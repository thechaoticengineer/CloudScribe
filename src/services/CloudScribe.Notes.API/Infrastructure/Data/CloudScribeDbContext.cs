using CloudScribe.Notes.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloudScribe.Notes.API.Infrastructure.Data;

public class CloudScribeDbContext(DbContextOptions<CloudScribeDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CloudScribeDbContext).Assembly);
    }
}