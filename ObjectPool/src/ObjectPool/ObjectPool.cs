using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ObjectPool
{
    /// <summary>
    /// Inspired by: https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-create-an-object-pool
    /// </summary>
    public class ObjectPool<T>
    {
        private int _createdObjectsCount = 0;
        public int CreatedObjectsCount => _createdObjectsCount;

        private long _returnedObjectsCount = 0;
        public long ReturnedObjectsCount => _returnedObjectsCount;

        public int PoolSize => _pool.Count;

        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();
        private readonly Func<T> _objectFactory;

        /// <summary>
        /// Create a new ObjectPool.
        /// </summary>
        /// <param name="objectFactory">The factory method used to create new instances when there aren't any available.</param>
        public ObjectPool(Func<T> objectFactory)
        {
            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        }

        /// <summary>
        /// If available, return an instance from the pool, otherwise a new one is created.
        /// </summary>
        /// <param name="forceNewInstance">Forces the creation of a new instance of the object, event if another one is available in the pool.</param>
        /// <returns>An instance.</returns>
        public T Get(bool forceNewInstance = false)
        {
            // An object is always returned.
            Interlocked.Increment(ref _returnedObjectsCount);

            if (!forceNewInstance && _pool.TryTake(out T instance))
                return instance;

            Interlocked.Increment(ref _createdObjectsCount);
            return _objectFactory();
        }

        /// <summary>
        /// Return a DisposablePoolObject that self releases the instance to the ObjectPool when disposed.
        /// </summary>
        /// <param name="forceNewInstance">Forces the creation of a new instance of the object, event if another one is available in the pool.</param>
        /// <returns>An instance wrapped in a DisposablePoolObject.</returns>
        public DisposablePoolObject<T> GetDisposable(bool forceNewInstance = false) => new DisposablePoolObject<T>(this, forceNewInstance);

        /// <summary>
        /// Put an instance in the ObjectPool, that can be used by other consumers.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Put(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _pool.Add(instance);
        }
    }

    /// <summary>
    /// Automatically releases an ObjectPool instance when disposed.
    /// The instance is lazily created.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    public class DisposablePoolObject<T> : IDisposable
    {
        public T Instance => _lazyInstance.Value;

        private Lazy<T> _lazyInstance;
        private ObjectPool<T> _pool;

        internal DisposablePoolObject(ObjectPool<T> pool, bool forceNewInstance = false)
        {
            _lazyInstance = new Lazy<T>(() => _pool.Get(forceNewInstance));
            _pool = pool;
        }

        /// <summary>
        /// If created, release the instance to the referenced ObjectPool.
        /// </summary>
        public void Dispose()
        {
            if (_lazyInstance.IsValueCreated)
                _pool.Put(_lazyInstance.Value);

            _lazyInstance = null;
            _pool = null;
            GC.SuppressFinalize(this);
        }
    }
}
