using Microsoft.Extensions.DependencyInjection;
#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#endif

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

#if NET6_0_OR_GREATER
    /// <summary>
    /// Configures webhook signature verification for use with the [EnsureIsFromLicenseManagement] attribute.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the webhook options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // In Program.cs
    /// builder.Services.AddLicenseManagementWebhooks(options =>
    /// {
    ///     options.Secret = builder.Configuration["Webhooks:Secret"];
    /// });
    ///
    /// // In your controller
    /// [HttpPost("webhook")]
    /// [EnsureIsFromLicenseManagement]
    /// public IActionResult HandleWebhook([FromBody] WebhookPayload payload)
    /// {
    ///     // Request is verified - process the webhook
    ///     return Ok();
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddLicenseManagementWebhooks(
        this IServiceCollection services,
        Action<WebhookOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Configures webhook signature verification from IConfiguration.
    /// Reads from the "LicenseManagement:Webhook" section by default.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">Optional section name (default: "LicenseManagement:Webhook").</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // In appsettings.json
    /// {
    ///   "LicenseManagement": {
    ///     "Webhook": {
    ///       "Secret": "whsec_your_secret_here"
    ///     }
    ///   }
    /// }
    ///
    /// // In Program.cs
    /// builder.Services.AddLicenseManagementWebhooks(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddLicenseManagementWebhooks(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = WebhookOptions.SectionName)
    {
        services.Configure<WebhookOptions>(configuration.GetSection(sectionName));
        return services;
    }
#endif
}
