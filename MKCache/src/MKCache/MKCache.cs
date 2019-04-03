using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MKCache
{
    public class MKCache<T>
    {
        public delegate object KeyIdentifier(T item);

        public bool ReuseRunningAsyncFetchers { get; set; } = true;

        public int CachesCount => _useDefaultCache ? 1 : _memoryCaches.Count;

        protected MemoryCache _defaultCache;
        protected List<(MemoryCache cache, KeyIdentifier kid)> _memoryCaches;

        protected readonly ConcurrentDictionary<object, Task<T>> _runningAsyncFetchers = new ConcurrentDictionary<object, Task<T>>();

        protected readonly bool _useDefaultCache;
        protected readonly MemoryCacheOptions _options;

        public MKCache(params KeyIdentifier[] keyIdentifiers)
            : this(null, keyIdentifiers?.AsEnumerable() ?? Array.Empty<KeyIdentifier>())
        {
        }

        public MKCache(MemoryCacheOptions options, params KeyIdentifier[] keyIdentifiers)
            : this(options, keyIdentifiers?.AsEnumerable() ?? Array.Empty<KeyIdentifier>())
        {
        }

        public MKCache(MemoryCacheOptions options, IEnumerable<KeyIdentifier> keyIdentifiers)
        {
            _options = options ?? new MemoryCacheOptions();

            if (keyIdentifiers.Any())
            {
                _memoryCaches = keyIdentifiers.Select(kid => (new MemoryCache(_options), kid)).ToList();
                _useDefaultCache = false;
            }
            else
            {
                _defaultCache = new MemoryCache(_options);
                _useDefaultCache = true;
            }
        }

        public virtual T Get(object key)
        {
            var (_, item) = TryFindInCache(key);
            return item;
        }

        public virtual void Clear()
        {
            if (_useDefaultCache)
            {
                _defaultCache.Dispose();
                _defaultCache = new MemoryCache(_options);
            }
            else
            {
                _memoryCaches = _memoryCaches.Select(x =>
                {
                    x.cache?.Dispose(); // Dispose previous cache
                    return (new MemoryCache(_options), x.kid);
                }).ToList();
            }
        }

        #region Sync

        public virtual T GetOrCreate(string key, Func<T> factory, TimeSpan expirationRelativeToUtcNow)
        {
            var (found, item) = TryFindInCache(key);
            if (found) return item;

            // Item not found.

            item = factory();
            this.Add(key, item, expirationRelativeToUtcNow);

            return item;
        }

        public virtual T GetOrCreate(string key, Func<T> factory, Func<TimeSpan> expirationDelegate) =>
            this.GetOrCreate(key, factory, item => expirationDelegate());

        public virtual T GetOrCreate(string key, Func<T> factory, Func<T, TimeSpan> expirationDelegate)
        {
            var (found, item) = TryFindInCache(key);
            if (found) return item;

            // Item not found.

            item = factory();
            var expirationRelativeToUtcNow = expirationDelegate(item);
            this.Add(key, item, expirationRelativeToUtcNow);

            return item;
        }

        #endregion

        #region Async

        public virtual async Task<T> GetOrCreateAsync(object key, Func<Task<T>> asyncFactory, TimeSpan expirationRelativeToUtcNow)
        {
            var (found, item) = TryFindInCache(key);
            if (found) return item;

            // Item not found.

            item = await FetchAsync(key, asyncFactory);
            this.Add(key, item, expirationRelativeToUtcNow);

            return item;
        }

        public virtual Task<T> GetOrCreateAsync(object key, Func<Task<T>> asyncFactory, Func<TimeSpan> expirationDelegate) =>
            this.GetOrCreateAsync(key, asyncFactory, item => expirationDelegate());

        public virtual async Task<T> GetOrCreateAsync(object key, Func<Task<T>> asyncFactory, Func<T, TimeSpan> expirationDelegate)
        {
            var (found, item) = TryFindInCache(key);
            if (found) return item;

            // Item not found.

            item = await FetchAsync(key, asyncFactory);
            var expirationRelativeToUtcNow = expirationDelegate(item);
            this.Add(key, item, expirationRelativeToUtcNow);

            return item;
        }

        #endregion

        protected virtual (bool found, T item) TryFindInCache(object key)
        {
            if (_useDefaultCache)
            {
                return TryFindInCache(_defaultCache);
            }
            else
            {
                foreach (var (cache, _) in _memoryCaches)
                {
                    var (found, item) = TryFindInCache(cache);
                    if (found) return (found, item);
                }
            }
            
            return (false, default(T));

            // Local function

            (bool found, T item) TryFindInCache(MemoryCache cache) =>
                cache.TryGetValue<T>(key, out var item) ? (true, item) : (false, default(T));
        }

        protected virtual void Add(object defaultKey, T item, TimeSpan expirationRelativeToUtcNow)
        {
            if (item == null) return;

            if (_useDefaultCache)
            {
                _defaultCache.Set(defaultKey, item, expirationRelativeToUtcNow);
            }
            else
            {
                foreach (var (cache, kid) in _memoryCaches)
                {
                    object key = kid(item);
                    if (key != null)
                        cache.Set(key, item, expirationRelativeToUtcNow);
                }
            }
        }

        private async Task<T> FetchAsync(object key, Func<Task<T>> asyncFactory)
        {
            var runningAsyncFinderKey = $"fetch_async_{key}";

            if (this.ReuseRunningAsyncFetchers)
            {
                // Check if another async request for the same object is already running

                if (_runningAsyncFetchers.TryGetValue(runningAsyncFinderKey, out var runningTask))
                {
                    // Another async finder for the same key is running!
                    return await runningTask.ConfigureAwait(false);
                }
            }
            
            // This key has no async finder running.
            var asyncFetcherTask = asyncFactory();
            
            // Add the newly created task to the dictionary, so that other consumers can reuse it (if configured).
            _runningAsyncFetchers.TryAdd(runningAsyncFinderKey, asyncFetcherTask);

            var result = await asyncFetcherTask.ConfigureAwait(false);
            
            // Remove the completed task
            _runningAsyncFetchers.TryRemove(runningAsyncFinderKey, out var _);

            return result;
        }
    }
}
