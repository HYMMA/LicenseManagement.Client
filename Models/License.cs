namespace LicenseManagement.Client.Models;

/// <summary>
/// Represents a software license for a specific product and computer.
/// </summary>
public class License
{
    /// <summary>
    /// The unique identifier (ULID) of the license.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The maximum number of days this license can be used as a trial.
    /// </summary>
    public int MaxTrialDays { get; set; } = 21;

    /// <summary>
    /// When this license expires (trial period end or receipt expiration).
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// The receipt indicating this license has been purchased. Null if trial.
    /// </summary>
    public Receipt? Receipt { get; set; }

    /// <summary>
    /// The product this license grants access to.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// The computer this license is assigned to.
    /// </summary>
    public Computer? Computer { get; set; }

    /// <summary>
    /// Whether this is a paid license (has receipt) or trial license.
    /// </summary>
    public bool IsTrial => Receipt == null;

    /// <summary>
    /// Whether this license has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > Expires;
}
