using Microsoft.Extensions.DependencyInjection;

namespace LicenseManagement.Client.Extensions;

/// <summary>
/// Extension methods for registering the License Management client with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the License Management client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>();
        return services;
    }

    /// <summary>
    /// Adds the License Management client to the service collection with a custom HTTP client configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <param name="configureClient">Action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions,
        Action<HttpClient> configureClient)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>(configureClient);
        return services;
    }

    /// <summary>
    /// Adds the License Management client to the service collection with a custom HTTP client builder configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <param name="configureBuilder">Action to configure the HTTP client builder (for adding handlers, etc.).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions,
        Action<IHttpClientBuilder> configureBuilder)
    {
        services.Configure(configureOptions);
        var builder = services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>();
        configureBuilder(builder);
        return services;
    }
}
