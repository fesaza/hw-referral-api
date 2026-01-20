using System.Text.Json.Serialization;

namespace CartonCaps.ReferralApi.Models;

/// <summary>
/// Represents a user in the system, either a referrer or a referee.
/// Centralizes user information that is not specific to referrals.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Referrals created by this user (as referrer).
    /// </summary>
    [JsonIgnore] //ignore this property when serializing to json to avoid circular references
    public virtual ICollection<Referral> ReferralsCreated { get; set; } = new List<Referral>();

    /// <summary>
    /// Referrals where this user was referred (as referee).
    /// </summary>
    [JsonIgnore] //ignore this property when serializing to json to avoid circular references
    public virtual ICollection<Referral> ReferralsReceived { get; set; } = new List<Referral>();
}
