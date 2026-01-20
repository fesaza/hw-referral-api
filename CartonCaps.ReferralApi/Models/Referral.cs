namespace CartonCaps.ReferralApi.Models;

/// <summary>
/// Represents a referral created by a user to invite friends.
/// </summary>
public class Referral
{

    public Guid Id { get; set; }

    public string ReferralCode { get; set; } = string.Empty;

    public Guid ReferrerUserId { get; set; }

    public virtual User? ReferrerUser { get; set; }

    public ReferralStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? InstalledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? RefereeUserId { get; set; }

    public virtual User? RefereeUser { get; set; }

    /// <summary>
    /// Deep link url
    /// </summary>
    public string ShareableLink { get; set; } = string.Empty;
}
