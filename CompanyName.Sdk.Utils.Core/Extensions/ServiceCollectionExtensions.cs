using CompanyName.Sdk.Utils.Core.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CompanyName.Sdk.Utils.Core.Extensions;

public static class ServiceCollectionExtensions
{
    private const string RequiredCacheSizeLimit = "Please provide a valid value for 'LruCache:SizeLimit' in the configuration.";

    public static IServiceCollection AddUtilsCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.GetSection("LruCache").GetValue<int?>("SizeLimit") is null)
        {
            throw new OptionsValidationException("LRUCache:SizeLimit", typeof(int), new List<string> { RequiredCacheSizeLimit });
        }

        services.AddOptions<LruCacheConfiguration>()
            .Bind(configuration.GetSection("LruCache"))
            .Validate(c => c.SizeLimit > 0, RequiredCacheSizeLimit);

        services.AddSingleton(typeof(ILruCache<,>), typeof(LruCache<,>));

        return services;
    }
}
