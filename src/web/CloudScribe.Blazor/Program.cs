using CloudScribe.Blazor.Components;
using CloudScribe.Blazor.Endpoints;
using CloudScribe.Blazor.Services;
using CloudScribe.Blazor.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices();

// Configure PostgreSQL connection for Data Protection Keys
var pgPassword = builder.Configuration["POSTGRES_PASSWORD"] ??
                 throw new InvalidOperationException("PostgreSQL password not configured");
var pgConnectionString = $"Host=postgres-service;Port=5432;Database=cloudscribe;Username=postgres;Password={pgPassword}";

builder.Services.AddDbContext<DataProtectionKeyDbContext>(options =>
{
    options.UseNpgsql(pgConnectionString);
    options.UseSnakeCaseNamingConvention();
});

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<DataProtectionKeyDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP cookies
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var keycloakSettings = builder.Configuration.GetSection("Keycloak");
        var publicUrl = builder.Configuration["PublicUrl"];

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
        options.SignedOutRedirectUri = "/";

        // Disable PAR (Pushed Authorization Request) - not needed for HTTP
        options.PushedAuthorizationBehavior = Microsoft.AspNetCore.Authentication.OpenIdConnect.PushedAuthorizationBehavior.Disable;

        // Set explicit callback path for redirect
        options.CallbackPath = "/signin-oidc";

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
                // Override redirect_uri to use public URL when configured
                if (!string.IsNullOrEmpty(publicUrl))
                {
                    context.ProtocolMessage.RedirectUri = $"{publicUrl}/signin-oidc";
                }

                // Replace internal host with public host in IssuerAddress
                var internalHost = new Uri(internalAddress).Authority;
                var publicHost = new Uri(publicAddress).Authority;

                context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress?
                    .Replace(internalHost, publicHost);

                if (context.Options.Configuration is not null)
                {
                    context.Options.Configuration.PushedAuthorizationRequestEndpoint = null;
                }

                return Task.CompletedTask;
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                var internalHost = new Uri(internalAddress).Authority;
                var publicHost = new Uri(publicAddress).Authority;

                if (context.ProtocolMessage.IssuerAddress != null)
                {
                    context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress
                        .Replace(internalHost, publicHost);
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                    .LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
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

// Create the DB table for Data Protection keys if it doesn't exist.
// This must be done before builder.Build() to avoid a race condition.
var tempServices = builder.Services.BuildServiceProvider();
var dbContext = tempServices.GetRequiredService<DataProtectionKeyDbContext>();
dbContext.Database.EnsureCreated();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

// Disable HTTPS redirection for cloud deployment
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapLoginAndLogout();

app.Run();