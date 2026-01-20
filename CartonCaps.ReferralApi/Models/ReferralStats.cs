namespace CartonCaps.ReferralApi.Models;

/// <summary>
/// Represents statistics about a user's referrals.
/// </summary>
public class ReferralStats
{
    /// <summary>
    /// Total number of referrals created by the user.
    /// </summary>
    public int TotalReferrals { get; set; }

    /// <summary>
    /// Number of pending referrals.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Number of referrals where the app was installed.
    /// </summary>
    public int InstalledCount { get; set; }

    /// <summary>
    /// Number of completed referrals.
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// Number of cancelled referrals.
    /// </summary>
    public int CancelledCount { get; set; }
}
