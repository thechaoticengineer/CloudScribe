using Duende.IdentityModel.Client;

namespace CloudScribe.Blazor.Services.Auth;

public class TokenService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<TokenService> logger)
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public void Initialize(string accessToken, string? refreshToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }
    
    public async Task<string?> GetAccessTokenAsync()
    {
        if (DateTimeOffset.UtcNow.AddMinutes(1) < ExpiresAt)
        {
            return AccessToken;
        }
        return await RefreshTokenAsync();
    }

    private async Task<string?> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(RefreshToken)) return null;

        var client = httpClientFactory.CreateClient();
        var oidcSettings = configuration.GetSection("Oidc");
        var authority = oidcSettings["Authority"];
        
        var discovery = await client.GetDiscoveryDocumentAsync(authority);
        if (discovery.IsError)
        {
            logger.LogError("Failed to refresh token: {Error}", discovery.Error);
            return null;
        }

        var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = discovery.TokenEndpoint,
            ClientId = oidcSettings["ClientId"]!,
            ClientSecret = oidcSettings["ClientSecret"]!,
            RefreshToken = RefreshToken
        });

        if (response.IsError)
        {
            logger.LogError($"Refresh Error: {response.Error}");
            return null;
        }
        
        AccessToken = response.AccessToken;
        RefreshToken = response.RefreshToken ?? RefreshToken;
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
        
        return AccessToken;
    }
}