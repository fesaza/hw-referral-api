using CartonCaps.ReferralApi.Data;
using CartonCaps.ReferralApi.Middleware;
using CartonCaps.ReferralApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Entity Framework Core with InMemory database
builder.Services.AddDbContext<ReferralDbContext>(options =>
    options.UseInMemoryDatabase("ReferralDb"));

// Register referral service as scoped (required for DbContext)
builder.Services.AddScoped<IReferralService, ReferralService>();

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<MockAuthenticationMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace CartonCaps.ReferralApi
{
    // Make Program class accessible for testing
    public partial class Program { }
}
