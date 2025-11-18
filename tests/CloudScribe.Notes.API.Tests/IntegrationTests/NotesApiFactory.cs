using CloudScribe.Notes.API.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    public PostgreSqlContainer DbContainer => _dbContainer;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
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
}
