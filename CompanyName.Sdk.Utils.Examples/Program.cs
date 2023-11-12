using CompanyName.Sdk.Utils.Core;
using CompanyName.Sdk.Utils.Core.Extensions;
using CompanyName.Sdk.Utils.Examples.Models;
using CompanyName.Sdk.Utils.Examples.Utils;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = NLog.LogLevel;

var logger = LogManager.GetCurrentClassLogger();
logger.Log(LogLevel.Info, "Starting LRU Memory Cache example application...");
ConsoleUtils.WriteInfoMessage("Starting LRU Memory Cache example application...\n");

try
{
    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
        .Build();

    using var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services
                .AddLogging(loggingBuilder =>
                {
                    // Configure Logging with NLog.
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog("nlog.config");

                }).BuildServiceProvider();

            services.AddUtilsCoreServices(configuration);

        })
        .Build();

    // Setup Cache and handler for Evicted events
    var personCache = ActivatorUtilities.CreateInstance<LruCache<Guid, Person>>(host.Services);
    personCache.CacheItemEvicted += HandleEvictedEvent;

    // Setup three keys
    var firstRecordKey = Guid.NewGuid();
    var secondRecordKey = Guid.NewGuid();
    var thirdRecordKey = Guid.NewGuid();

    // Add two records, accessing the first record last to force second record into the position of lru record.
    personCache.Set(firstRecordKey, new Person { FirstName = "John", LastName = "Doe" });
    personCache.Set(secondRecordKey, new Person { FirstName = "Mary", LastName = "Smith" });
    personCache.TryGetValue(secondRecordKey, out var secondRecordValue);
    Console.WriteLine($"Retrieve 2nd record: {secondRecordValue} \n");
    personCache.TryGetValue(firstRecordKey, out var firstRecordValue);
    Console.WriteLine($"Retrieve 1st record: {firstRecordValue} \n");

    // Add third record to force eviction of second record.
    personCache.Set(thirdRecordKey, new Person { FirstName = "Ann", LastName = "Barry" });
    personCache.TryGetValue(thirdRecordKey, out var thirdRecordValue);
    Console.WriteLine($"Retrieve 3rd record: {thirdRecordValue} \n");

    var secondRecordStillExists = personCache.TryGetValue(secondRecordKey, out _);
    Console.WriteLine($"Cache contains secondRecord: {secondRecordStillExists}");

    void HandleEvictedEvent(object? sender, KeyValuePair<Guid, Person> cacheEntry)
    {
        ConsoleUtils.WriteEvictedMessage($"{cacheEntry} has been evicted.\n");
    }

    Console.ReadLine();
    await host.RunAsync();
}
catch (OptionsValidationException ex)
{
    logger.Log(LogLevel.Error, ex.Message);
    ConsoleUtils.WriteEvictedMessage(ex.Message);
}
catch (ArgumentNullException ex)
{
    logger.Log(LogLevel.Error, ex.Message);
    ConsoleUtils.WriteEvictedMessage(ex.Message);
}
finally
{
    LogManager.Shutdown();
}




