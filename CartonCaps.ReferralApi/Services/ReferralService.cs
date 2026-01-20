using CartonCaps.ReferralApi.Data;
using CartonCaps.ReferralApi.DTOs;
using CartonCaps.ReferralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.Services;

/// <summary>
/// Service implementation for managing referrals using Entity Framework Core.
/// Uses InMemory database for development and testing.
/// </summary>
public class ReferralService : IReferralService
{
    private readonly ReferralDbContext _context;
    private readonly Random _random = new();

    public ReferralService(ReferralDbContext context)
    {
        _context = context;
        InitializeMockData();
    }

    public async Task<List<Referral>> GetUserReferralsAsync(Guid userId)
    {
        var referrals = await _context.Referrals
            .Include(r => r.ReferrerUser)
            .Include(r => r.ReferrerUser)
            .Where(r => r.ReferrerUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return referrals;
    }

    public async Task<Referral?> GetReferralByIdAsync(Guid referralId)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.Id == referralId);

        return referral ?? null;
    }

    public async Task<ReferralDetailsResponse?> GetReferralByCodeAsync(string referralCode)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == referralCode);
        
        if (referral == null)
        {
            return null;
        }

        var response = new ReferralDetailsResponse
        {
            Id = referral.Id,
            ReferralCode = referral.ReferralCode,
            ReferrerUserId = referral.ReferrerUserId,
            Status = referral.Status,
            CreatedAt = referral.CreatedAt,
            IsValid = referral.Status != ReferralStatus.Cancelled
        };

        return response;
    }

    public async Task<Referral> CreateReferralAsync(Guid userId, CreateReferralRequest request)
    {
        // Ensure referrer user exists (This is only for testing to avoid Users controller)
        var referrerUser = await _context.Users.FindAsync(userId);
        if (referrerUser == null)
        {
            referrerUser = new User
            {
                Id = userId
            };
            _context.Users.Add(referrerUser);
        }

        // Create referee user if email provided
        User? refereeUser = null;
        if (!string.IsNullOrWhiteSpace(request.RefereeEmail))
        {
            refereeUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.RefereeEmail);

            if (refereeUser == null)
            {
                refereeUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.RefereeEmail
                };
                _context.Users.Add(refereeUser);
            }
        }

        var referralId = Guid.NewGuid();
        var referralCode = GenerateReferralCode();
        var shareableLink = GenerateShareableLink(referralCode);

        var referral = new Referral
        {
            Id = referralId,
            ReferralCode = referralCode,
            ReferrerUserId = userId,
            Status = ReferralStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RefereeUserId = refereeUser?.Id,
            ShareableLink = shareableLink
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        return referral;
    }

    public async Task<ReferralStats> GetReferralStatsAsync(Guid userId)
    {
        var referrals = await _context.Referrals
            .Where(r => r.ReferrerUserId == userId)
            .ToListAsync();

        var stats = new ReferralStats
        {
            TotalReferrals = referrals.Count,
            PendingCount = referrals.Count(r => r.Status == ReferralStatus.Pending),
            InstalledCount = referrals.Count(r => r.Status == ReferralStatus.Installed),
            CompletedCount = referrals.Count(r => r.Status == ReferralStatus.Completed),
            CancelledCount = referrals.Count(r => r.Status == ReferralStatus.Cancelled)
        };

        return stats;
    }

    public async Task<bool> MarkReferralAsInstalledAsync(string referralCode)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == referralCode);
        
        if (referral == null || referral.Status > ReferralStatus.Installed)
        {
            return false;
        }

        referral.Status = ReferralStatus.Installed;
        referral.InstalledAt = DateTime.UtcNow;
        referral.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkReferralAsCompletedAsync(string referralCode, Guid refereeUserId)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == referralCode);
        
        if (referral == null || referral.Status > ReferralStatus.Completed)
        {
            return false;
        }

        referral.Status = ReferralStatus.Completed;
        referral.CompletedAt = DateTime.UtcNow;
        referral.RefereeUserId = refereeUserId;
        referral.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateReferralCode()
    {
        // Generate a realistic referral code (e.g., "REF-ABC123XYZ")
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new string(Enumerable.Repeat(chars, 9)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
        return $"REF-{code}";
    }

    private string GenerateShareableLink(string referralCode)
    {
        return $"https://cartoncaps.app/refer/{referralCode}";
    }

    private void InitializeMockData()
    {
        // Only initialize if database is empty
        if (_context.Referrals.Any() || _context.Users.Any())
        {
            return;
        }

        // Create mock users
        var userId1 = Guid.Parse(Constants.DefaultMockUserId);
        var userId2 = Guid.Parse("87654321-4321-4321-4321-210987654321");

        var user1 = new User
        {
            Id = userId1,
            Email = "user1@example.com",
            Name = "User One"
        };

        var user2 = new User
        {
            Id = userId2,
            Email = "user2@example.com",
            Name = "User Two"
        };

        _context.Users.AddRange(user1, user2);

        var referrals = new List<Referral>();
        var refereeUsers = new List<User>();

        // User 1 has 5 referrals with various statuses
        for (int i = 0; i < 5; i++)
        {
            var referralId = Guid.NewGuid();
            var referralCode = GenerateReferralCode();
            var status = (ReferralStatus)(i % 4);
            var createdAt = DateTime.UtcNow.AddDays(-(10 - i));

            User? refereeUser = null;
            if (i % 2 == 0)
            {
                var refereeId = Guid.NewGuid();
                refereeUser = new User
                {
                    Id = refereeId,
                    Email = $"friend{i}@example.com"
                };
                refereeUsers.Add(refereeUser);
            }

            var referral = new Referral
            {
                Id = referralId,
                ReferralCode = referralCode,
                ReferrerUserId = userId1,
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                ShareableLink = GenerateShareableLink(referralCode),
                RefereeUserId = refereeUser?.Id
            };

            if (status == ReferralStatus.Installed || status == ReferralStatus.Completed)
            {
                referral.InstalledAt = createdAt.AddHours(2);
            }

            if (status == ReferralStatus.Completed)
            {
                referral.CompletedAt = createdAt.AddDays(1);
                if (refereeUser == null)
                {
                    refereeUser = new User
                    {
                        Id = Guid.NewGuid()
                    };
                    refereeUsers.Add(refereeUser);
                }
                referral.RefereeUserId = refereeUser.Id;
            }

            referrals.Add(referral);
        }

        // User 2 has 2 referrals
        for (int i = 0; i < 2; i++)
        {
            var referralId = Guid.NewGuid();
            var referralCode = GenerateReferralCode();
            var status = i == 0 ? ReferralStatus.Pending : ReferralStatus.Installed;
            var createdAt = DateTime.UtcNow.AddDays(-(5 - i));

            var referral = new Referral
            {
                Id = referralId,
                ReferralCode = referralCode,
                ReferrerUserId = userId2,
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                ShareableLink = GenerateShareableLink(referralCode)
            };

            if (status == ReferralStatus.Installed)
            {
                referral.InstalledAt = createdAt.AddHours(1);
            }

            referrals.Add(referral);
        }

        _context.Users.AddRange(refereeUsers);
        _context.Referrals.AddRange(referrals);
        _context.SaveChanges();
    }
}
