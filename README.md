# LRU Cache 
A size-limited least-recently-used (LRU) generic Cache.

## Getting Started
_Note: In the real world this would be implemented as a **Nuget** package._

1.	Add configuration to specify size limit of Cache.
2.	Use the **AddUtilsCoreServices** Service Collection extension to inject the required dependencies
3.	(optional) Register your preferred Logging library.
4.  Start Adding/Retrieving entries with the cache!

The example provided uses a Console Application rather than a Web API to demo the Cache functionality.
The Example uses NLog - creating log files in the `.\bin\Debug\net6.0\Logs` folder.

## Usage
1. Add configuration to specify the LruCache:SizeLimit.
```json
{
  "LruCache": {
    "SizeLimit": "2"
  }
}
```
*Size limit should be greater than zero.* 

2. Register the LruCache dependencies by calling **AddUtilsCoreServices** referencing the configuration.

```csharp
    using var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddUtilsCoreServices(configuration);
        })
        .Build();
```

3. Instantiate the Cache specifying the type of Key and Value to be stored  (LruCache<TKey, TValue>)

```csharp
var cache = ActivatorUtilities.CreateInstance<LruCache<string, string>>(serviceCollection);
```

4. To subscribe to Cache Eviction events you can reference the **CacheItemEvicted** EventHandler

```csharp
cache.CacheItemEvicted += CustomEvictedEventHandler;
```

5.  Add and Retrieve Cache entries using **Set** and **TryGetValue**

```csharp
cache.Set("key", "value");
cache.TryGetValue("key");
```

Note that the `Set` method will 
* `evict` the least recently used cache item once the cache size limit threshold is passed
* `overwrite` the value of an existing cache entry if the key parameter matches an existing key


### References Used
* [Microsoft MemoryCache](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache?view=dotnet-plat-ext-7.0&viewFallbackFrom=net-6.0)

* [Microsoft IMemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache?view=dotnet-plat-ext-7.0)

* [Finbourne Multimap](https://github.com/finbourne/lusid-sdk-csharp/blob/main/sdk/Lusid.Sdk/Client/Multimap.cs)



