using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CloudScribe.Blazor.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("authentication");
        
        group.MapGet("/login", ([FromQuery] string? returnUrl) =>
        {
            var properties = GetAuthProperties(returnUrl);
            return TypedResults.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
        });
        
        group.MapPost("/logout", ([FromQuery] string? returnUrl) =>
        {
            var properties = GetAuthProperties(returnUrl);
            return TypedResults.SignOut(properties, 
                [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
        });

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        const string pathBase = "/";

        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith(pathBase))
        {
            returnUrl = pathBase;
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}