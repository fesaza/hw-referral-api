using CartonCaps.ReferralApi;
using CartonCaps.ReferralApi.Data;
using CartonCaps.ReferralApi.DTOs;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CartonCaps.ReferralApi.Tests;

public class ReferralServiceTests
{
    private IReferralService CreateService()
    {
        var options = new DbContextOptionsBuilder<ReferralDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new ReferralDbContext(options);
        return new ReferralService(context);
    }

    [Fact]
    public async Task GetUserReferralsAsync_WithValidUserId_ReturnsReferrals()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);

        // Act
        var result = await service.GetUserReferralsAsync(userId);

        // Assert
        Assert.NotNull(result);
        var referrals = result.ToList();
        Assert.True(referrals.Count > 0);
        Assert.All(referrals, r => Assert.NotEqual(Guid.Empty, r.Id));
        Assert.All(referrals, r => Assert.NotNull(r.ReferralCode));
    }

    [Fact]
    public async Task GetUserReferralsAsync_WithInvalidUserId_ReturnsEmpty()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();

        // Act
        var result = await service.GetUserReferralsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateReferralAsync_WithValidRequest_CreatesReferral()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);
        var userReferralCode = "USER-ABC123";
        var request = new CreateReferralRequest
        {
            RefereeEmail = "test@example.com",
        };

        // Act
        var result = await service.CreateReferralAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEmpty(result.ReferralCode);
        Assert.Equal(ReferralStatus.Pending, result.Status);
        Assert.Equal(request.RefereeEmail, request.RefereeEmail);
        Assert.NotEmpty(result.ShareableLink);
    }

    [Fact]
    public async Task GetReferralByCodeAsync_WithValidCode_ReturnsReferral()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);
        var userReferralCode = "USER-ABC123";
        var request = new CreateReferralRequest();
        var created = await service.CreateReferralAsync(userId, request);

        // Act
        var result = await service.GetReferralByCodeAsync(created.ReferralCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.ReferralCode, result.ReferralCode);
        Assert.Equal(userId, result.ReferrerUserId);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task GetReferralByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var result = await service.GetReferralByCodeAsync("INVALID-CODE");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetReferralStatsAsync_WithValidUserId_ReturnsStats()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);

        // Act
        var result = await service.GetReferralStatsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalReferrals >= 0);
        Assert.True(result.PendingCount >= 0);
        Assert.True(result.InstalledCount >= 0);
        Assert.True(result.CompletedCount >= 0);
        Assert.True(result.CancelledCount >= 0);
    }

    [Fact]
    public async Task MarkReferralAsInstalledAsync_WithValidCode_UpdatesStatus()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);
        var userReferralCode = "USER-ABC123";
        var request = new CreateReferralRequest();
        var created = await service.CreateReferralAsync(userId, request);

        // Act
        var result = await service.MarkReferralAsInstalledAsync(created.ReferralCode);

        // Assert
        Assert.True(result);
        
        var updated = await service.GetReferralByCodeAsync(created.ReferralCode);
        Assert.NotNull(updated);
        Assert.Equal(ReferralStatus.Installed, updated.Status);
    }

    [Fact]
    public async Task MarkReferralAsCompletedAsync_WithValidCode_UpdatesStatus()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.Parse(Constants.DefaultMockUserId);
        var userReferralCode = "USER-ABC123";
        var request = new CreateReferralRequest();
        var created = await service.CreateReferralAsync(userId, request);
        var refereeUserId = Guid.NewGuid();

        // Act
        var result = await service.MarkReferralAsCompletedAsync(created.ReferralCode, refereeUserId);

        // Assert
        Assert.True(result);
        
        var updated = await service.GetReferralByCodeAsync(created.ReferralCode);
        Assert.NotNull(updated);
        Assert.Equal(ReferralStatus.Completed, updated.Status);
    }

    [Fact]
    public async Task MarkReferralAsInstalledAsync_WithInvalidCode_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var result = await service.MarkReferralAsInstalledAsync("INVALID-CODE");

        // Assert
        Assert.False(result);
    }
}
