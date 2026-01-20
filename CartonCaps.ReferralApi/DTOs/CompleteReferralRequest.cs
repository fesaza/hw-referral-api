using System.ComponentModel.DataAnnotations;

namespace CartonCaps.ReferralApi.Controllers;

public class CompleteReferralRequest
{
    /// <summary>
    /// The ID of the user who completed the referral
    /// </summary>
    [Required]
    public Guid RefereeUserId { get; set; }
}