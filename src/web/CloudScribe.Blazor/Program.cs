using CloudScribe.Blazor.Components;
using CloudScribe.Blazor.Endpoints;
using CloudScribe.Blazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Oidc:Authority"];
        options.ClientId = builder.Configuration["Oidc:ClientId"];
        options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
        options.ResponseType = "code";
        options.RequireHttpsMetadata = false;
        options.SaveTokens = true;
        
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var accessToken = context.SecurityToken.RawData;
                
                var identity = (System.Security.Claims.ClaimsIdentity)context.Principal!.Identity!;
                identity.AddClaim(new System.Security.Claims.Claim("access_token", accessToken));
            
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddHttpClient<NotesClient>(client =>
{
    client.BaseAddress =
        new Uri(builder.Configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl is missing"));
});
//.AddHttpMessageHandler<AuthorizationMessageHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapLoginAndLogout();

app.Run();