using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<AIJob> AIJobs { get; set; } = null!;

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentContent> DocumentContents => Set<DocumentContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AIJob>()
            .Property(x => x.JobType)
            .HasConversion<string>();

        modelBuilder.Entity<AIJob>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DocumentContent>()
            .HasOne(x => x.Document)
            .WithOne()
            .HasForeignKey<DocumentContent>(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}