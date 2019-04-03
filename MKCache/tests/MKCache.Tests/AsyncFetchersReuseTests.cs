using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MKCache.Tests
{
    public class AsyncFetchersReuseTests
    {
        private TimeSpan GetExpiration() => TimeSpan.FromMinutes(5);

        [Fact]
        public async Task Running_async_fetchers_are_reused()
        {
            var item = new Item();

            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(() => Task.Run(async () =>
            {
                // A time long enough for the test to request n concurrent async factories invocations
                await Task.Delay(TimeSpan.FromSeconds(2));
                return item;
            }));

            var cache = new MKCache<Item>
            {
                ReuseRunningAsyncFetchers = true
            };

            var foundItemTask = cache.GetOrCreateAsync("any", asyncFactoryMock.Object, GetExpiration);

            // Simulate concurrent request for the same object (same key), while the item is not yet been cached.
            var concurrentFinders = new List<Task<Item>>();
            var n = 100;
            for (int i = 0; i < n; i++)
                concurrentFinders.Add(cache.GetOrCreateAsync("any", asyncFactoryMock.Object, GetExpiration));

            var foundItem = await foundItemTask;
            // The delegate is invoked only once
            asyncFactoryMock.Verify(asyncFinder => asyncFinder(), Times.Once);

            // The reused and completed task gets mapped into another task for each call (by the cache awaiting it),
            // therefore we might still be needing to await them to complete.
            await Task.WhenAll(concurrentFinders);
            // Even if the Id gets created new each time, the same result has been returned.
            concurrentFinders.ForEach(t => Assert.Equal(t.Result.Id, foundItem.Id));

            // After the async fetcher has completed, the item is stored in cache
            // so the delegate still won't be invoked (but for a different reason).

            var cachedItem = await cache.GetOrCreateAsync("any", asyncFactoryMock.Object, GetExpiration);
            Assert.Same(item, cachedItem);
            asyncFactoryMock.Verify(asyncFinder => asyncFinder(), Times.Once);
        }

        [Fact]
        public async Task Running_async_fetchers_are_not_reused_by_configuration()
        {
            var asyncFactoryMock = new Mock<Func<Task<Item>>>();
            asyncFactoryMock.Setup(asyncFactory => asyncFactory()).Returns(() => Task.Run(async () =>
            {
                // A time long enough for the test to request n concurrent async factories invocations
                await Task.Delay(TimeSpan.FromSeconds(2));
                return new Item();
            }));

            var cache = new MKCache<Item>
            {
                ReuseRunningAsyncFetchers = false
            };

            var foundItemTask = cache.GetOrCreateAsync("any", asyncFactoryMock.Object, GetExpiration);

            // Simulate concurrent request for the same object (same key), while the item is not yet been cached.
            var concurrentFinders = new List<Task<Item>>();
            var n = 100;
            for (int i = 0; i < n; i++)
                concurrentFinders.Add(cache.GetOrCreateAsync("any", asyncFactoryMock.Object, GetExpiration));

            var foundItem = await foundItemTask;            
            await Task.WhenAll(concurrentFinders);
            // Tasks have not been reused, therefore each item is different.
            concurrentFinders.ForEach(t => Assert.NotEqual(t.Result.Id, foundItem.Id));

            // Ensure that all the delegates have been invoked.
            asyncFactoryMock.Verify(asyncFinder => asyncFinder(), Times.Exactly(n + 1));
        }
    }
}
