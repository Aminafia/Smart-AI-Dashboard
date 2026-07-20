using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<AIJob> AIJobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AIJob>()
                .Property(x => x.JobType)
                .HasConversion<string>();

            modelBuilder.Entity<AIJob>()
                .Property(x => x.Status)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
