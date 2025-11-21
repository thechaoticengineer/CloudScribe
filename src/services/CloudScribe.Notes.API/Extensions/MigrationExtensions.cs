using CloudScribe.Notes.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudScribe.Notes.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CloudScribeDbContext>();

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }
    
}