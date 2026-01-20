using CartonCaps.ReferralApi;

namespace CartonCaps.ReferralApi.Middleware;

/// <summary>
/// Mock authentication middleware for development purposes.
/// In a real application, this would validate JWT tokens or session cookies.
/// </summary>
public class MockAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public MockAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract user ID from header (mock authentication)
        // In production, this would come from a validated JWT token or session
        if (context.Request.Headers.TryGetValue(Constants.UserIdHeader, out var userId))
        {
            var userIdString = userId.ToString();
            if (Guid.TryParse(userIdString, out var parsedUserId))
            {
                context.Items["UserId"] = parsedUserId.ToString();
            }
            else
            {
                // If invalid format, use default
                context.Items["UserId"] = Guid.Parse(Constants.DefaultMockUserId).ToString();
            }
        }
        else
        {
            // Default user for testing if no header is provided
            context.Items["UserId"] = Guid.Parse(Constants.DefaultMockUserId).ToString();
        }

        await _next(context);
    }
}
