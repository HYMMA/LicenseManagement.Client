using LicenseManagement.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LicenseManagement.Client.Extensions;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the M-10 fix: WebhookOptions.TimestampTolerance is validated at startup.
/// </summary>
public class WebhookOptionsValidatorTests
{
    [Fact]
    public void Validator_Rejects_Zero_Tolerance()
    {
        var services = new ServiceCollection();
        services.AddLicenseManagementWebhooks(o =>
        {
            o.Secret = "whsec_x";
            o.TimestampTolerance = TimeSpan.Zero;
        });
        using var sp = services.BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<WebhookOptions>>();
        Assert.Throws<OptionsValidationException>(() => opts.Value);
    }

    [Fact]
    public void Validator_Rejects_Negative_Tolerance()
    {
        var services = new ServiceCollection();
        services.AddLicenseManagementWebhooks(o =>
        {
            o.Secret = "whsec_x";
            o.TimestampTolerance = TimeSpan.FromSeconds(-1);
        });
        using var sp = services.BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<WebhookOptions>>();
        Assert.Throws<OptionsValidationException>(() => opts.Value);
    }

    [Fact]
    public void Validator_Rejects_Tolerance_Over_One_Hour()
    {
        var services = new ServiceCollection();
        services.AddLicenseManagementWebhooks(o =>
        {
            o.Secret = "whsec_x";
            o.TimestampTolerance = TimeSpan.FromHours(2);
        });
        using var sp = services.BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<WebhookOptions>>();
        Assert.Throws<OptionsValidationException>(() => opts.Value);
    }

    [Fact]
    public void Validator_Accepts_Reasonable_Tolerance()
    {
        var services = new ServiceCollection();
        services.AddLicenseManagementWebhooks(o =>
        {
            o.Secret = "whsec_x";
            o.TimestampTolerance = TimeSpan.FromMinutes(5);
        });
        using var sp = services.BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<WebhookOptions>>();
        Assert.Equal(TimeSpan.FromMinutes(5), opts.Value.TimestampTolerance);
    }

    [Fact]
    public void Validator_Rejects_Zero_MaxBodyBytes()
    {
        var services = new ServiceCollection();
        services.AddLicenseManagementWebhooks(o =>
        {
            o.Secret = "whsec_x";
            o.MaxBodyBytes = 0;
        });
        using var sp = services.BuildServiceProvider();

        var opts = sp.GetRequiredService<IOptions<WebhookOptions>>();
        Assert.Throws<OptionsValidationException>(() => opts.Value);
    }
}
