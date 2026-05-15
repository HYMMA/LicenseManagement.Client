using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#endif

namespace LicenseManagement.Client.Extensions;

/// <summary>
/// Extension methods for registering the License Management client with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly string UserAgentValue = BuildUserAgent();

    /// <summary>
    /// Adds the License Management client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <returns>The HTTP client builder, so callers can add Polly handlers (recommended).</returns>
    /// <remarks>
    /// The recommended companion is a Polly retry policy on transient failures (5xx, 429) that
    /// honours the <c>Retry-After</c> header returned by the API. Example:
    /// <code>
    /// services.AddLicenseManagementClient(o => { ... })
    ///         .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
    /// </code>
    /// </remarks>
    public static IHttpClientBuilder AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>(ConfigureClient);
    }

    /// <summary>
    /// Adds the License Management client to the service collection with extra HTTP client configuration.
    /// The supplied delegate runs <em>after</em> the SDK's own configuration so callers can add or
    /// override headers without losing the SDK's <c>X-API-KEY</c>, <c>Accept</c>, and <c>User-Agent</c>.
    /// </summary>
    public static IHttpClientBuilder AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions,
        Action<HttpClient> configureClient)
    {
        services.Configure(configureOptions);
        return services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>((sp, http) =>
        {
            ConfigureClient(sp, http);
            configureClient(http);
        });
    }

    /// <summary>
    /// Adds the License Management client to the service collection with custom builder configuration
    /// (handlers, message handlers, Polly policies, etc.).
    /// </summary>
    public static IServiceCollection AddLicenseManagementClient(
        this IServiceCollection services,
        Action<LicenseManagementClientOptions> configureOptions,
        Action<IHttpClientBuilder> configureBuilder)
    {
        services.Configure(configureOptions);
        var builder = services.AddHttpClient<ILicenseManagementClient, LicenseManagementClient>(ConfigureClient);
        configureBuilder(builder);
        return services;
    }

    private static void ConfigureClient(IServiceProvider sp, HttpClient http)
    {
        var opts = sp.GetRequiredService<IOptions<LicenseManagementClientOptions>>().Value;

        if (!string.IsNullOrEmpty(opts.BaseUrl))
        {
            http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
        }

        if (opts.TimeoutSeconds > 0)
        {
            http.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        }

        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/problem+json"));

        if (!string.IsNullOrEmpty(opts.ApiKey))
        {
            if (http.DefaultRequestHeaders.Contains("X-API-KEY"))
                http.DefaultRequestHeaders.Remove("X-API-KEY");
            http.DefaultRequestHeaders.Add("X-API-KEY", opts.ApiKey);
        }

        http.DefaultRequestHeaders.UserAgent.Clear();
        http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentValue);
    }

    private static string BuildUserAgent()
    {
        var version = typeof(LicenseManagementClient).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(LicenseManagementClient).Assembly.GetName().Version?.ToString()
            ?? "0.0.0";

        // Strip metadata after '+' for a cleaner UA token.
        var plus = version.IndexOf('+');
        if (plus > 0) version = version.Substring(0, plus);

        return $"LicenseManagement.Client/{version}";
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
        services.AddSingleton<IValidateOptions<WebhookOptions>, WebhookOptionsValidator>();
        return services;
    }

    /// <summary>
    /// Configures webhook signature verification from IConfiguration.
    /// Reads from the "LicenseManagement:Webhook" section by default.
    /// </summary>
    public static IServiceCollection AddLicenseManagementWebhooks(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = WebhookOptions.SectionName)
    {
        services.Configure<WebhookOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IValidateOptions<WebhookOptions>, WebhookOptionsValidator>();
        return services;
    }
#endif
}
