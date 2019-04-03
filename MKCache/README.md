# MKCache

Almost called *yamc*, this is *yet another memory cache*.
<br />
This type is a thin layer of abstraction over `Microsoft.Extensions.Caching.Memory.MemoryCache`
(.NET standard) that **allows to cache an element using more than one key**.

## Scenario

Given the following type:

```csharp
class Country
{
    // Id unique in the database.
    public int Id { get; set; }

    public string Name { get; set; }

    // ISO 3166-2 standard, unique identifier.
    public string ISOCode { get; set; }
}
```

allow the cache consumers to find the cached item either by `Id` or `ISOCode`,
since they both identify uniquely the entity, and **their values don't overlap**,
meaning that no `Id` will never equal to an `ISOCode`.

## Use

#### with a default key

```csharp
// This cache doesn't have any multi-key logic.
// It behaves exactly like a MemoryCache.
var cache = new MKCache<Country>();
```

#### with one or more specific keys

```csharp
var cache = new MKCache<Country>(
    // Cache the items using both their Id and their ISOCode.
    c => c.Id,
    c => c.ISOCode);

// The key: in this case, it could be either the Id or the ISOCode.
var key = "US";

// The delegate that will retrieve the country, if it's not found in the cache.
async Task<Country> countryResolver()
{
    // e.g. an http call
    return await _countriesService.ResolveAsync("US");
};

var countryCacheExpiration = TimeSpan.FromMinutes(30);

var country = await cache.GetOrCreateAsync(
    key,
    countryResolver,
    countryCacheExpiration);

// Now the country can be found in the cache using both its Id and its ISOCode
var countryFoundById = await cache.GetOrCreateAsync(country.Id, countryResolver, countryCacheExpiration);
Assert(countryFoundById != null);

// even synchronously.
var countryFoundByISO = cache.GetOrCreate("US", () => _countriesService.Resolve("US"), countryCacheExpiration);
Assert(countryFoundByISO != null);
```

Check out the [samples](./samples/) for more use cases.
<br />

## Reusing running asynchronous fetchers

Sometimes it may happen that the cache is requested to resolve an item with the same key multiple times, *concurrently*.
<br/>
When the cache doesn't have a reference to the item yet, this would cause it to run as many item-resolution-delegates
as the cache invocations.

```csharp
var people = new[]
{
    new Person { Name = "Thomas", CountryISOCode = "US", },
    new Person { Name = "Astrid", CountryISOCode = "NO", },
    new Person { Name = "Elizabeth", CountryISOCode = "US", },
};

// The country "US" will be requested two times,
// and if the cache doesn't hold the country's reference
// _countriesService.ResolveAsync("US") will be invoked two times.
var allCountryNames = await Task.WhenAll(people.Select(async p =>
{
    return await cache.GetOrCreateAsync(
        p.CountryISOCode,
        () => _countriesService.ResolveAsync(p.CountryISOCode),
        TimeSpan.FromMinutes(30));
}));

var uniqueCountryNames = allCountryNames.Distinct();
```

In order to mitigate this, the `MKCache` reuses the tasks created by the delegates, which are stored into a `ConcurrentDictionary`.
<br />
This won't *ensure* that two or more concurrent requests with the same key will never be executed,
because **there's no lock** in play, but in general it will greatly improve the use of resources
and performances, proportionally to the amount of "twin" requests executed concurrently.

This behavior can be disabled by setting `cache.ReuseRunningAsyncFetchers = false` (default is `true`).
