using AutoBogus;
using CompanyName.Sdk.Utils.Core;
using CompanyName.Sdk.Utils.Core.Configurations;
using Microsoft.Extensions.Options;
using Moq;

namespace CompanyName.Sdk.Utils.UnitTests;

public sealed class LruCacheTests
{
    private readonly Mock<IOptions<LruCacheConfiguration>> _options;

    public LruCacheTests()
    {
        _options = new Mock<IOptions<LruCacheConfiguration>>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void LruCache_ValidCacheSizeLimit_InstantiatesSuccessfully(int cacheSizeLimit)
    {
        // Arrange & Act
        var lruCache = GetLruCache<string, string>(cacheSizeLimit);

        // Assert
        lruCache.Should().NotBeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void LruCache_InvalidCacheSizeLimit_ThrowsArgumentException(int cacheSizeLimit)
    {
        // Arrange & Act
        var lruCache = () => GetLruCache<string, string>(cacheSizeLimit);

        // Assert
        lruCache.Should().Throw<ArgumentException>().Where(ex =>
            ex.ParamName == "SizeLimit" &&
            ex.Message.Contains("Cache size limit must be greater than zero."));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("key", null)]
    [InlineData(null, "value")]
    public void Set_InvalidParameters_ThrowsArgumentNullException(string key, string value)
    {
        // Arrange
        var lruCache = GetLruCache<string, string>(1);

        // Act
        var act = () => lruCache.Set(key, value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("key", "value")]
    [InlineData("2379d113-aa2d-41b1-afa3-c8e53fc85252", 100)]
    public void Set_ValidParametersWithinCacheSizeLimit_CreatesNewCacheEntry<TKey, TValue>(string key, TValue value)
    {
        // Arrange
        var lruCache = GetLruCache<string, TValue>(10);

        // Act
        lruCache.Set(key, value);

        // Assert
        lruCache.TryGetValue(key, out var entryValue);
        entryValue.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void Set_WithinCacheSizeLimit_CreatesNewCacheEntry()
    {
        // Arrange
        var key = Guid.NewGuid();
        var person = new Person { FirstName = "John", LastName = "Doe" };
        var lruCache = GetLruCache<Guid, Person>(10);

        // Act
        lruCache.Set(key, person);

        // Assert
        lruCache.TryGetValue(key, out var entryValue);
        entryValue.Should().BeEquivalentTo(person);
    }

    [Fact]
    public void Set_WhenExceedsCacheSizeLimit_CreatesNewCacheEntryEvictsOldestEntry()
    {
        // Arrange
        var lruCache = GetLruCache<int, string>(2);

        // Act
        lruCache.Set(1, "value1");
        lruCache.Set(2, "value2");
        lruCache.Set(3, "value3");

        // Assert
        lruCache.TryGetValue(1, out _).Should().BeFalse();
        lruCache.TryGetValue(2, out var value2).Should().BeTrue();
        value2.Should().Be("value2");
        lruCache.TryGetValue(3, out var value3).Should().BeTrue();
        value3.Should().Be("value3");
    }

    [Fact]
    public void Set_WhenCacheSizeLimitIsExceeded_TryGetValueCallAltersOrderingEvictsOldestEntry()
    {
        // Arrange
        var lruCache = GetLruCache<int, string>(2);

        // Act
        lruCache.Set(1, "value1");
        lruCache.Set(2, "value2");
        lruCache.TryGetValue(1, out _);
        lruCache.Set(3, "value3");

        // Assert
        lruCache.TryGetValue(2, out _).Should().BeFalse();
        lruCache.TryGetValue(1, out var value1).Should().BeTrue();
        value1.Should().Be("value1");
        lruCache.TryGetValue(3, out var value3).Should().BeTrue();
        value3.Should().Be("value3");
    }

    [Fact]
    public void Set_WhenKeyExists_UpdatesExistingEntry()
    {
        // Arrange
        var lruCache = GetLruCache<int, string>(3);

        // Act
        lruCache.Set(1, "value1");
        lruCache.Set(2, "value2");
        lruCache.Set(3, "value3");
        lruCache.Set(1, "updatedValue1");

        // Assert
        lruCache.TryGetValue(1, out var value).Should().BeTrue();
        value.Should().Be("updatedValue1");
    }

    private LruCache<TKey, TValue> GetLruCache<TKey, TValue>(int cacheSizeLimit) where TKey : notnull
    {
        var options = AutoFaker.Generate<LruCacheConfiguration>();
        options.SizeLimit = cacheSizeLimit;
        _options.Setup(x => x.Value).Returns(options);
        return new LruCache<TKey, TValue>(_options.Object);
    }

    internal record Person
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}


