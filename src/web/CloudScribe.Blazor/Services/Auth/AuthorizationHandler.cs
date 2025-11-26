using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace CloudScribe.Blazor.Services.Auth;

public class AuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext ??
                          throw new InvalidOperationException("No httpcontext available from the iHttpContextAccessor");
        
        var accessToken = await httpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}