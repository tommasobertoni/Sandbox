using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MKCache.Tests
{
    public class MultiKeysTests
    {
        private TimeSpan GetExpiration() => TimeSpan.FromMinutes(5);

        [Fact]
        public void An_item_is_cached_with_multiple_keys()
        {
            var item = new Item();
            var key = item.Id;

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var cache = new MKCache<Item>(
                // Multi-key cache rules.
                x => x.Id,
                x => x.Name);

            var foundItem = cache.GetOrCreate(key, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.GetOrCreate(key, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItem);

            var cachedItemWithDifferentKey = cache.GetOrCreate(item.Name, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItemWithDifferentKey);
        }

        [Fact]
        public async Task An_item_is_cached_with_multiple_keys_async()
        {
            var item = new Item();
            var key = item.Id;

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>(
                // Multi-key cache rules.
                x => x.Id,
                x => x.Name);

            var foundItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItem);

            var cachedItemWithDifferentKey = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItemWithDifferentKey);
        }

        [Fact]
        public async Task Multi_key_cache_can_be_cleared()
        {
            var item = new Item();
            var key = item.Id;

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>(
                // Multi-key cache rules.
                x => x.Id,
                x => x.Name);

            var foundItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItem);

            cache.Clear();

            var _ = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }
    }
}
