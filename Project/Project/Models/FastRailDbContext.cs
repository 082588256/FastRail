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

        public DbSet<Seat> Seats { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Ticket> Ticket { get; set; }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<TicketSegment> TicketSegments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Route>().ToTable("Route");
            modelBuilder.Entity<RouteSegment>().ToTable("RouteSegment");
            modelBuilder.Entity<Train>().ToTable("Train");
            modelBuilder.Entity<Carriage>().ToTable("Carriage");
            modelBuilder.Entity<Seat>().ToTable("Seat");
            modelBuilder.Entity<Trip>().ToTable("Trip");
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }
    
    }

