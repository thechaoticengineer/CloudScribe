using System.Text;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Tests.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace CloudScribe.Notes.API.Tests.IntegrationTests;

public class NotesApiFactory : WebApplicationFactory<AssemblyMarker>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cloudscribe_test")
        .WithPortBinding(5432, true)
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task StartContainerAsync()
    {
        await _dbContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestAuthHandler.Issuer,
            
                    ValidateAudience = false,
            
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestAuthHandler.TestSecretKey)),
            
                    ValidateLifetime = true
                };
                
                options.ConfigurationManager = null;
                options.MetadataAddress = null!;
                options.Authority = null;
            });
            
            services.RemoveAll<DbContextOptions<CloudScribeDbContext>>();
            services.RemoveAll<CloudScribeDbContext>();
            
            services.AddDbContext<CloudScribeDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
            
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CloudScribeDbContext>();
            dbContext.Database.Migrate();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
