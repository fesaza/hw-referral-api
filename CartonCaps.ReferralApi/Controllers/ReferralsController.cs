using System.ComponentModel.DataAnnotations;
using CartonCaps.ReferralApi.DTOs;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartonCaps.ReferralApi.Controllers;

/// <summary>
/// Controller for managing referral operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReferralsController : ControllerBase
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    /// <summary>
    /// Gets all referrals for the current user.
    /// </summary>
    /// <returns>List of referrals for the authenticated user.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Referral>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<Referral>>> GetUserReferrals()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User ID is required");
        }

        var referrals = await _referralService.GetUserReferralsAsync(userId.Value);
        return Ok(referrals);
    }

    /// <summary>
    /// Gets a specific referral by ID.
    /// </summary>
    /// <param name="id">The referral ID.</param>
    /// <returns>The referral if found and authorized.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Referral), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Referral>> GetReferralById(string id)
    {
        if (!Guid.TryParse(id, out var referralId))
        {
            return BadRequest("Invalid referral ID format.");
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User ID is required");
        }

        var referral = await _referralService.GetReferralByIdAsync(referralId);
        if (referral == null)
        {
            return NotFound($"Referral with ID '{id}' not found or you don't have access to it.");
        }

        return Ok(referral);
    }

    /// <summary>
    /// Gets referral details by referral code. Used for deep linking when a user clicks a referral link.
    /// </summary>
    /// <param name="code">The referral code.</param>
    /// <returns>The referral details if found.</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ReferralDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReferralDetailsResponse>> GetReferralByCode(string code)
    {
        var referral = await _referralService.GetReferralByCodeAsync(code);
        if (referral == null)
        {
            return NotFound($"Referral code '{code}' not found.");
        }

        return Ok(referral);
    }

    /// <summary>
    /// Creates a new referral for the current user.
    /// </summary>
    /// <param name="request">The referral creation request.</param>
    /// <returns>The created referral.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Referral), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Referral>> CreateReferral([FromBody] CreateReferralRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User ID is required");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var referral = await _referralService.CreateReferralAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetReferralById), new { id = referral.Id }, referral);
    }

    /// <summary>
    /// Gets referral statistics for the current user.
    /// </summary>
    /// <returns>Referral statistics.</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ReferralStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReferralStats>> GetReferralStats()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized("User ID is required");
        }

        var stats = await _referralService.GetReferralStatsAsync(userId.Value);
        return Ok(stats);
    }

    /// <summary>
    /// Marks a referral as installed. Called when a referred user installs the app.
    /// </summary>
    /// <param name="code">The referral code.</param>
    /// <returns>Success status.</returns>
    [HttpPost("code/{code}/installed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkReferralAsInstalled(string code)
    {
        var success = await _referralService.MarkReferralAsInstalledAsync(code);
        if (!success)
        {
            return NotFound($"Referral code '{code}' not found or is invalid.");
        }

        return Ok(new { message = "Referral marked as installed successfully." });
    }

    /// <summary>
    /// Marks a referral as completed. Called when a referred user completes registration.
    /// </summary>
    /// <param name="code">The referral code.</param>
    /// <param name="request">Request containing the referee user ID.</param>
    /// <returns>Success status.</returns>
    [HttpPost("code/{code}/completed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkReferralAsCompleted(string code, [FromBody] CompleteReferralRequest request)
    {
        if (request.RefereeUserId == Guid.Empty)
        {
            return BadRequest("Referee user ID is required.");
        }

        var success = await _referralService.MarkReferralAsCompletedAsync(code, request.RefereeUserId);
        if (!success)
        {
            return NotFound($"Referral code '{code}' not found or is invalid.");
        }

        return Ok(new { message = "Referral marked as completed successfully." });
    }

    private Guid? GetUserId()
    {
        var userIdString = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userIdString))
        {
            return null;
        }

        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        return null;
    }
}

public class CompleteReferralRequest
{
    /// <summary>
    /// The ID of the user who completed the referral (the referee).
    /// </summary>
    [Required]
    public Guid RefereeUserId { get; set; }
}
