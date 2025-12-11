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
    /// When the trial period ends for this license.
    /// </summary>
    public DateTime TrialEndDate { get; set; }

    /// <summary>
    /// When this license file expires (cache expiration).
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
    /// Key-value metadata attached to this license by the publisher.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Whether this is a paid license (has receipt) or trial license.
    /// </summary>
    public bool IsTrial => Receipt == null;

    /// <summary>
    /// Whether this license has expired (cache expiration).
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > Expires;

    /// <summary>
    /// Whether the trial period has ended.
    /// </summary>
    public bool IsTrialExpired => DateTime.UtcNow > TrialEndDate;
}
