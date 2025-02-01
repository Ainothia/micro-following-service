using FollowService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FollowingService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Follower> Followers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follower>()
                .HasIndex(f => new { f.FollowerId, f.FolloweeId })
                .IsUnique();
        }
    }
}