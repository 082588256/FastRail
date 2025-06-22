using Microsoft.EntityFrameworkCore;
namespace Project.Models

{
    public class FastRailDbContext : DbContext
    {
        public FastRailDbContext(DbContextOptions<FastRailDbContext> options) : base(options)
        {
        }
        public DbSet<Train> Trains { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteSegment> RouteSegments { get; set; }
        public DbSet<Carriage> Carriages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration can go here
        }
    }
    
    }

