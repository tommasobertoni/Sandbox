using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MKCache.Tests
{
    public class InputHandlingTests
    {
        private TimeSpan GetExpiration() => TimeSpan.FromMinutes(5);

        [Fact]
        public void Items_can_be_retrieved_from_the_cache()
        {
            var item = new Item();

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var cache = new MKCache<Item>();

            var nullItem = cache.Get("any");
            Assert.Null(nullItem);

            var foundItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            var cachedItem = cache.Get("any");
            Assert.Same(item, cachedItem);
        }

        [Fact]
        public void Null_value_does_not_throw_and_does_not_get_cached()
        {
            var cache = new MKCache<Item>();

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(null as Item);

            var nullItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Null(nullItem);

            nullItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
            Assert.Null(nullItem);
        }

        [Fact]
        public async Task Null_value_does_not_throw_and_does_not_get_cached_async()
        {
            var cache = new MKCache<Item>();

            var factoryMock = new Mock<Func<Task<Item>>>();
            factoryMock.Setup(factory => factory()).Returns(Task.FromResult(null as Item));

            var nullItem = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Null(nullItem);

            nullItem = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
            Assert.Null(nullItem);
        }

        [Fact]
        public void Null_key_from_identifier_does_not_throw_and_value_does_not_get_cached()
        {
            var cache = new MKCache<Item>(
                x => x.Name /* The Name is the key */);

            var item = new Item { Name = null };

            var factoryMock = new Mock<Func<Item>>();
            factoryMock.Setup(factory => factory()).Returns(item);

            var foundItem = cache.GetOrCreate(item.Id, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            foundItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
            Assert.Same(item, foundItem);

            foundItem = cache.GetOrCreate("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(3));
            Assert.Same(item, foundItem);
        }

        [Fact]
        public async Task Null_key_from_identifier_does_not_throw_and_value_does_not_get_cached_async()
        {
            var cache = new MKCache<Item>(
                x => x.Name /* The Name is the key */);

            var item = new Item { Name = null };

            var factoryMock = new Mock<Func<Task<Item>>>();
            factoryMock.Setup(factory => factory()).Returns(Task.FromResult(item));

            var foundItem = await cache.GetOrCreateAsync(item.Id, factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Once);
            Assert.Same(item, foundItem);

            foundItem = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(2));
            Assert.Same(item, foundItem);

            foundItem = await cache.GetOrCreateAsync("any", factoryMock.Object, GetExpiration);
            factoryMock.Verify(factory => factory(), Times.Exactly(3));
            Assert.Same(item, foundItem);
        }
    }
}
