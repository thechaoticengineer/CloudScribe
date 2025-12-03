using CloudScribe.Notes.API.Api.Endpoints;
using CloudScribe.Notes.API.ExceptionHandling;
using CloudScribe.Notes.API.Extensions;
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
        var authSettings = builder.Configuration.GetSection("Authentication");
        
        options.Authority = authSettings["Authority"];
        options.MetadataAddress = authSettings["MetadataAddress"]!;
        options.RequireHttpsMetadata = bool.Parse(authSettings["RequireHttpsMetadata"]!);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "http://keycloak-service:8080/realms/cloudscribe",
                "http://localhost:8080/realms/cloudscribe",
                "http://localhost:8180/realms/cloudscribe"
            },
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapNotesEndpoints();
app.UseHttpsRedirection();
app.Run();