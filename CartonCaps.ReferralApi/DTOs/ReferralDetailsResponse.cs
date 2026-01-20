using CartonCaps.ReferralApi.Models;

namespace CartonCaps.ReferralApi.DTOs;

/// <summary>
/// Detailed response model for referral data, including referrer information.
/// Used when looking up a referral by code (e.g., for deep linking).
/// </summary>
public class ReferralDetailsResponse
{
    /// <summary>
    /// Unique identifier for the referral.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The referral code used to identify this referral.
    /// </summary>
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who created this referral (the referrer).
    /// </summary>
    public Guid ReferrerUserId { get; set; }

    /// <summary>
    /// Current status of the referral.
    /// </summary>
    public ReferralStatus Status { get; set; }

    /// <summary>
    /// Date and time when the referral was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Indicates whether this referral is still valid and can be used.
    /// </summary>
    public bool IsValid { get; set; }
}
