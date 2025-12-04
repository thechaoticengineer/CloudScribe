using CloudScribe.Blazor.Components;
using CloudScribe.Blazor.Endpoints;
using CloudScribe.Blazor.Services;
using CloudScribe.Blazor.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var keycloakSettings = builder.Configuration.GetSection("Keycloak");

        var publicAddress = keycloakSettings["PublicAddress"]!;
        var internalAddress = keycloakSettings["InternalAddress"]!;
        var realm = keycloakSettings["Realm"]!;
        var clientId = keycloakSettings["ClientId"]!;
        var clientSecret = keycloakSettings["ClientSecret"]!;

        options.Authority = $"{internalAddress}/realms/{realm}";
        options.MetadataAddress = $"{internalAddress}/realms/{realm}/.well-known/openid-configuration";
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.RequireHttpsMetadata = false;
        options.SaveTokens = true;

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");

        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.ValidIssuer = $"{publicAddress}/realms/{realm}";

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var accessToken = context.SecurityToken.RawData;

                var identity = (System.Security.Claims.ClaimsIdentity)context.Principal!.Identity!;
                identity.AddClaim(new System.Security.Claims.Claim("access_token", accessToken));

                return Task.CompletedTask;
            },
            OnRedirectToIdentityProvider = context =>
            {
                // Replace internal Keycloak address with public address for browser redirects
                var internalHost = new Uri(internalAddress).Authority;
                var publicHost = new Uri(publicAddress).Authority;

                context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress?
                    .Replace(internalHost, publicHost);

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<NotesClient>();

builder.Services.AddHttpClient("API", client => 
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]!);
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapLoginAndLogout();

app.Run();