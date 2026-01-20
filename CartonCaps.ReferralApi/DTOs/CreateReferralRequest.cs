using System.ComponentModel.DataAnnotations;

namespace CartonCaps.ReferralApi.DTOs;

public class CreateReferralRequest
{
    [EmailAddress]
    public string? RefereeEmail { get; set; }
}
