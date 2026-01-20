# CartonCaps Referral API

A .NET 9.0 Web API for managing referral programs.

## Prerequisites

- .NET 9.0 SDK or later

## 1. Restore and Build Solution

Restore packages:
```bash
dotnet restore CartonCaps.ReferralApi.sln
```

Build solution:
```bash
dotnet build CartonCaps.ReferralApi.sln
```

## 2. Execute Unit Tests

Run all tests:
```bash
dotnet test CartonCaps.ReferralApi.sln
```

## 3. Add HTTPS Certificate

Trust the .NET development certificate:
```bash
dotnet dev-certs https --trust
```

Run with HTTPS:
```bash
dotnet run --project CartonCaps.ReferralApi/CartonCaps.ReferralApi.csproj --launch-profile https
```

The API will be available at:
- HTTPS: `https://localhost:7131`
- HTTP: `http://localhost:5286`

## 4. Using .http File to Test API

1. Start the API:
   ```bash
   dotnet run --project CartonCaps.ReferralApi/CartonCaps.ReferralApi.csproj
   ```

2. Open `CartonCaps.ReferralApi/CartonCaps.ReferralApi.http` in your IDE

3. Click the "Send Request" link (VS Code) or green arrow (Visual Studio) above each request

**Note**: For HTTPS, update the `@CartonCaps.ReferralApi_HostAddress` variable to `https://localhost:7131`
