using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MKCache.Tests
{
    public class SingleKeyTests
    {
        private TimeSpan GetExpiration() => TimeSpan.FromMinutes(5);

        [Fact]
        public void An_item_is_cached_with_the_default_key()
        {
            var item = new Item();
            var key = item.Id;

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var cache = new MKCache<Item>();

            var foundItem = cache.GetOrCreate(key, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.GetOrCreate(key, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = cache.GetOrCreate(item.Name, factoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
        }

        [Fact]
        public async Task An_item_is_cached_with_the_default_key_async()
        {
            var item = new Item();
            var key = item.Id;

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>();

            var foundItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(key, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }

        [Fact]
        public async Task Default_key_cache_can_be_cleared()
        {
            var item = new Item();
            var key = item.Id;

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>();

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

        [Fact]
        public void An_item_is_cached_with_a_single_key()
        {
            var item = new Item();

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var cache = new MKCache<Item>(
                x => x.Name /* Name property is the key */);

            var foundItem = cache.GetOrCreate(item.Id, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.GetOrCreate(item.Name, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = cache.GetOrCreate(item.Id, factoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found by its Id
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
        }

        [Fact]
        public async Task An_item_is_cached_with_a_single_key_async()
        {
            var item = new Item();

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>(
                x => x.Name /* Name property is the key */);

            var foundItem = await cache.GetOrCreateAsync(item.Id, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = await cache.GetOrCreateAsync(item.Id, asyncFactoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found by its Id
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }

        [Fact]
        public async Task Single_key_cache_can_be_cleared()
        {
            var item = new Item();

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>(
                x => x.Name /* Name property is the key */);

            var foundItem = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Once);
            Assert.Same(item, cachedItem);

            cache.Clear();

            var _ = await cache.GetOrCreateAsync(item.Name, asyncFactoryMock.Object, GetExpiration);
            asyncFactoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }

        [Fact]
        public void An_item_is_not_found_with_the_default_key_is_a_specific_key_is_specified()
        {
            var name = "A test name";
            var item = new Item { Name = name };

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var cache = new MKCache<Item>(
                x => x.Name /* Name property is the key */);

            var foundItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.GetOrCreate(name, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found by its Name and the default key
            factoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }

        [Fact]
        public async Task An_item_is_not_found_with_the_default_key_is_a_specific_key_is_specified_async()
        {
            var name = "A test name";
            var item = new Item { Name = name };

            var factoryMock = new Mock<Func<Task<Item>>>();
            factoryMock.Setup(factory => factory()).Returns(Task.FromResult(item));

            var cache = new MKCache<Item>(
                x => x.Name /* Name property is the key */);

            var foundItem = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(name, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, cachedItem);

            var _ = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            // Delegate invoked a second time, because the item was not found by its Name and the default key
            factoryMock.Verify(asyncFactory => asyncFactory(), Times.Exactly(2));
        }
    }
}
