using System.Net.Http.Headers;

namespace CloudScribe.Blazor.Services;

public class AuthorizationMessageHandler(TokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(tokenProvider.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenProvider.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}