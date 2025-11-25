using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace CloudScribe.Blazor.Services;

public class AuthorizationMessageHandler(AuthenticationStateProvider authStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var tokenClaim = user.FindFirst("access_token");

        if (tokenClaim != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenClaim.Value);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}