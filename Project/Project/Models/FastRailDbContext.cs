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
            modelBuilder.Entity<Route>().ToTable("Route");
            modelBuilder.Entity<RouteSegment>().ToTable("RouteSegment");
            modelBuilder.Entity<Train>().ToTable("Train");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }
    
    }

