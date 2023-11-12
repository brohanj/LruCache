using System.Collections.Concurrent;
using CompanyName.Sdk.Utils.Core.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CompanyName.Sdk.Utils.Core;

/// <summary>
/// Represents a size-limited least-recently-used cache of key value pairs.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value associated with the key.</typeparam>
public class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _sizeLimit;
    private readonly object _cacheLock = new();

    private readonly ConcurrentDictionary<TKey, TValue> _cacheEntries;
    private readonly LinkedList<TKey> _lruKeyList;
    private readonly ILogger<LruCache<TKey, TValue>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LruCache{TKey,TValue}" /> class.
    /// </summary>
    /// <param name="lruCacheConfiguration">Configuration outlining Maximum size limit of the cache.</param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentException"></exception>
    public LruCache(IOptions<LruCacheConfiguration> lruCacheConfiguration, ILogger<LruCache<TKey, TValue>>? logger)
    {
        ArgumentNullException.ThrowIfNull(lruCacheConfiguration);

        var config = lruCacheConfiguration.Value;
        if (config.SizeLimit < 1)
        {
            logger?.LogError("Cache size limit must be greater than zero.");
            throw new ArgumentException("Cache size limit must be greater than zero.", nameof(config.SizeLimit));
        }

        _logger = logger;
        _sizeLimit = (lruCacheConfiguration.Value).SizeLimit;
        _cacheEntries = new ConcurrentDictionary<TKey, TValue>();
        _lruKeyList = new LinkedList<TKey>();
    }

    public LruCache(IOptions<LruCacheConfiguration> lruCacheConfiguration) : this(lruCacheConfiguration, null)
    {
    }

    public event EventHandler<KeyValuePair<TKey, TValue>>? CacheItemEvicted;

    public void OnEvictedCompleted(KeyValuePair<TKey, TValue> e)
    {
        CacheItemEvicted?.Invoke(this, e);
    }

    /// <summary>
    /// Add the key/value pair to the Cache if new, 
    /// otherwise update the value against the existing key.
    /// 
    /// If the cache size limit is exceeded, the least-recently-used entry is removed from the cache.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(TKey key, TValue value)
    {
        _logger?.LogTrace("Set {key} {value}", key, value);
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        lock (_cacheLock)
        {
            if (_cacheEntries.TryGetValue(key, out _))
            {
                _cacheEntries[key] = value;

                _lruKeyList.Remove(key);
                _lruKeyList.AddFirst(key);
            }
            else
            {
                if (_cacheEntries.Count >= _sizeLimit)
                {
                    EvictLruCacheEntry();
                }

                _cacheEntries[key] = value;
                _lruKeyList.AddFirst(key);
            }

            _logger?.LogInformation("Set - Added: {key}", key);
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with
    /// the specified key, if the key is found; otherwise, the default value for the 
    /// type of the <paramref name="value" /> parameter.</param>
    /// <returns>true if contains an element with the specified key; otherwise, false.</returns>	
    public bool TryGetValue(TKey key, out TValue? value)
    {
        _logger?.LogTrace("TryGetValue {key}", key);
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        lock (_cacheLock)
        {
            if (_cacheEntries.TryGetValue(key, out var entry))
            {
                _lruKeyList.Remove(key);
                _lruKeyList.AddFirst(key);

                _logger?.LogInformation("TryGetValue - Found: {key}", key);

                value = entry;
                return true;
            }

            _logger?.LogInformation("TryGetValue - Not Found: {key}", key);
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Evict the least-recently-used Cache entry.
    /// </summary>
    private void EvictLruCacheEntry()
    {
        lock (_cacheLock)
        {
            var lruKey = _lruKeyList.Last;
            if (lruKey is not null)
            {
                var item = new KeyValuePair<TKey, TValue>(lruKey.Value, _cacheEntries[lruKey.Value]);
                _cacheEntries.Remove(lruKey.Value, out _);
                _lruKeyList.RemoveLast();

                _logger?.LogWarning("Evicted: {key}", lruKey.Value);

                OnEvictedCompleted(item);
            }
        }
    }
}