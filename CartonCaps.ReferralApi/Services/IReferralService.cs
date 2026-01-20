using CartonCaps.ReferralApi.DTOs;
using CartonCaps.ReferralApi.Models;

namespace CartonCaps.ReferralApi.Services;

/// <summary>
/// Service interface for managing referrals.
/// </summary>
public interface IReferralService
{
    /// <summary>
    /// Gets all referrals for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>List of referrals for the user.</returns>
    Task<List<Referral>> GetUserReferralsAsync(Guid userId);

    /// <summary>
    /// Gets a specific referral by ID.
    /// </summary>
    /// <param name="referralId">The ID of the referral.</param>
    /// <returns>The referral if found and authorized, null otherwise.</returns>
    Task<Referral?> GetReferralByIdAsync(Guid referralId);

    /// <summary>
    /// Gets referral details by referral code. Used for deep linking.
    /// </summary>
    /// <param name="referralCode">The referral code.</param>
    /// <returns>The referral details if found, null otherwise.</returns>
    Task<ReferralDetailsResponse?> GetReferralByCodeAsync(string referralCode);

    /// <summary>
    /// Creates a new referral for a user.
    /// </summary>
    /// <param name="userId">The ID of the user creating the referral.</param>
    /// <param name="request">The referral creation request.</param>
    /// <returns>The created referral.</returns>
    Task<Referral> CreateReferralAsync(Guid userId, CreateReferralRequest request);

    /// <summary>
    /// Gets referral statistics for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>Referral statistics.</returns>
    Task<ReferralStats> GetReferralStatsAsync(Guid userId);

    /// <summary>
    /// Marks a referral as installed (when the referred user installs the app).
    /// </summary>
    /// <param name="referralCode">The referral code.</param>
    /// <returns>True if the referral was found and updated, false otherwise.</returns>
    Task<bool> MarkReferralAsInstalledAsync(string referralCode);

    /// <summary>
    /// Marks a referral as completed (when the referred user completes registration).
    /// </summary>
    /// <param name="referralCode">The referral code.</param>
    /// <param name="refereeUserId">The ID of the user who completed the referral.</param>
    /// <returns>True if the referral was found and updated, false otherwise.</returns>
    Task<bool> MarkReferralAsCompletedAsync(string referralCode, Guid refereeUserId);
}
