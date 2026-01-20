using System.Net;
using System.Net.Http.Json;
using System.Threading;
using CartonCaps.ReferralApi;
using CartonCaps.ReferralApi.Data;
using CartonCaps.ReferralApi.DTOs;
using CartonCaps.ReferralApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CartonCaps.ReferralApi.Tests;

public class ReferralsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static int _testCounter = 0;

    private HttpClient CreateClient()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ReferralDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add a new InMemory database for each test
                // Use a unique name per test to ensure isolation
                services.AddDbContext<ReferralDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: $"TestDb_{testId}");
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetUserReferrals_WithValidUser_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);

        // Act
        var response = await client.GetAsync("/api/referrals");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var referrals = await response.Content.ReadFromJsonAsync<List<Referral>>();
        Assert.NotNull(referrals);
    }

    [Fact]
    public async Task GetUserReferrals_WithoutUserHeader_ReturnsOkWithDefaultUser()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/referrals");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateReferral_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);
        var request = new CreateReferralRequest
        {
            RefereeEmail = "newfriend@example.com",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/referrals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var referral = await response.Content.ReadFromJsonAsync<Referral>();
        Assert.NotNull(referral);
        Assert.NotEqual(Guid.Empty, referral.Id);
        Assert.NotEmpty(referral.ReferralCode);
        Assert.NotEmpty(referral.ShareableLink);
    }

    [Fact]
    public async Task GetReferralByCode_WithValidCode_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);
        
        // Create a referral first
        var createRequest = new CreateReferralRequest();
        var createResponse = await client.PostAsJsonAsync("/api/referrals", createRequest);
        var createdReferral = await createResponse.Content.ReadFromJsonAsync<Referral>();

        // Act
        var response = await client.GetAsync($"/api/referrals/code/{createdReferral!.ReferralCode}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var referral = await response.Content.ReadFromJsonAsync<ReferralDetailsResponse>();
        Assert.NotNull(referral);
        Assert.Equal(createdReferral.ReferralCode, referral.ReferralCode);
    }

    [Fact]
    public async Task GetReferralByCode_WithInvalidCode_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/referrals/code/INVALID-CODE");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReferralStats_WithValidUser_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);

        // Act
        var response = await client.GetAsync("/api/referrals/stats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<ReferralStats>();
        Assert.NotNull(stats);
        Assert.True(stats.TotalReferrals >= 0);
    }

    [Fact]
    public async Task MarkReferralAsInstalled_WithValidCode_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);
        
        // Create a referral first
        var createRequest = new CreateReferralRequest();
        var createResponse = await client.PostAsJsonAsync("/api/referrals", createRequest);
        var createdReferral = await createResponse.Content.ReadFromJsonAsync<Referral>();

        // Act
        var response = await client.PostAsync(
            $"/api/referrals/code/{createdReferral!.ReferralCode}/installed",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MarkReferralAsCompleted_WithValidCode_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Constants.UserIdHeader, Constants.DefaultMockUserId);
        
        // Create a referral first
        var createRequest = new CreateReferralRequest();
        var createResponse = await client.PostAsJsonAsync("/api/referrals", createRequest);
        var createdReferral = await createResponse.Content.ReadFromJsonAsync<Referral>();

        var completeRequest = new { RefereeUserId = Guid.NewGuid() };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/referrals/code/{createdReferral!.ReferralCode}/completed",
            completeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
