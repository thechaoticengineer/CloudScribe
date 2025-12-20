using CloudScribe.Notes.API.Api.Endpoints;
using CloudScribe.Notes.API.Configuration;
using CloudScribe.Notes.API.ExceptionHandling;
using CloudScribe.Notes.API.HostedServices;
using CloudScribe.Notes.API.Infrastructure.Auth;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CloudScribeDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakSettings = builder.Configuration.GetSection("Keycloak");

        var publicAddress = keycloakSettings["PublicAddress"]!;
        var internalAddress = keycloakSettings["InternalAddress"]!;
        var realm = keycloakSettings["Realm"]!;

        var metadataAddress = $"{internalAddress}/realms/{realm}/.well-known/openid-configuration";
        var issuer = $"{publicAddress}/realms/{realm}";

        options.MetadataAddress = metadataAddress;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<NotesService>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.Configure<MigrationJobSettings>(
    builder.Configuration.GetSection("MigrationJob"));

builder.Services.AddHostedService<DatabaseMigrationHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapNotesEndpoints();

// Disable HTTPS redirection - handled by nginx Ingress
//app.UseHttpsRedirection();

app.Run();