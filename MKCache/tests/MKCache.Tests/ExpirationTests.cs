using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MKCache.Tests
{
    public class ExpirationTests
    {
        [Fact]
        public async Task Cache_item_expires()
        {
            var cache = new MKCache<Item>();

            var item = new Item();

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var expirySpan = TimeSpan.FromSeconds(2);

            var foundItem = cache.GetOrCreate(item.Id, factoryMock.Object, expirySpan);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.GetOrCreate(item.Id, () => new Item(), expirySpan);
            Assert.Same(item, cachedItem);

            await Task.Delay(expirySpan);

            var newItem = cache.GetOrCreate(item.Id, factoryMock.Object, expirySpan);
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
        }

        [Fact]
        public async Task Cache_item_expires_async()
        {
            var cache = new MKCache<Item>();

            var item = new Item();

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(Task.FromResult(item));

            var expirySpan = TimeSpan.FromSeconds(2);

            var foundItem = await cache.GetOrCreateAsync(item.Id, asyncFactoryMock.Object, expirySpan);
            asyncFactoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = await cache.GetOrCreateAsync(item.Id, () => Task.FromResult(new Item()), expirySpan);
            Assert.Same(item, cachedItem);

            await Task.Delay(expirySpan);

            var newItem = await cache.GetOrCreateAsync(item.Id, asyncFactoryMock.Object, expirySpan);
            asyncFactoryMock.Verify(factory => factory(), Times.Exactly(2));
        }
    }
}
