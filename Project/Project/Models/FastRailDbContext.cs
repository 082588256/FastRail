using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project
{
    public class FastRailDbContext : DbContext
    {
        public FastRailDbContext(DbContextOptions<FastRailDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<Station> Station { get; set; }
        public DbSet<Models.Route> Route { get; set; }
        public DbSet<RouteSegment> RouteSegment { get; set; }
        public DbSet<Train> Train { get; set; }
        public DbSet<Carriage> Carriage { get; set; }
        public DbSet<Seat> Seat { get; set; }
        public DbSet<Trip> Trip { get; set; }
        public DbSet<TripRouteSegment> TripRouteSegment { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<SeatSegment> SeatSegment { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketSegment> TicketSegment { get; set; }
        public DbSet<Fare> Fare { get; set; }
        public DbSet<Payment> Payment{ get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<PriceCalculationLog> PriceCalculationLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(255);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Route configurations
            modelBuilder.Entity<Models.Route>(entity =>
            {
                entity.HasKey(e => e.RouteId);
                entity.Property(e => e.RouteName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.RouteCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TotalDistance).HasColumnType("decimal(10,2)");

                entity.HasOne(d => d.DepartureStation)
                      .WithMany(p => p.DepartureRoutes)
                      .HasForeignKey(d => d.DepartureStationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ArrivalStation)
                      .WithMany(p => p.ArrivalRoutes)
                      .HasForeignKey(d => d.ArrivalStationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.RouteCode).IsUnique();
            });

            // RouteSegment configurations
            modelBuilder.Entity<RouteSegment>(entity =>
            {
                entity.HasKey(e => e.SegmentId);
                entity.Property(e => e.Distance).HasColumnType("decimal(10,2)").IsRequired();

                entity.HasOne(d => d.Route)
                      .WithMany(p => p.RouteSegments)
                      .HasForeignKey(d => d.RouteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.FromStation)
                      .WithMany(p => p.FromSegments)
                      .HasForeignKey(d => d.FromStationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToStation)
                      .WithMany(p => p.ToSegments)
                      .HasForeignKey(d => d.ToStationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.RouteId, e.Order }).IsUnique();
            });

            // SeatSegment configurations
            modelBuilder.Entity<SeatSegment>(entity =>
            {
                entity.HasKey(e => e.SeatSegmentId);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Available");

                entity.HasOne(d => d.Trip)
                      .WithMany(p => p.SeatSegments)
                      .HasForeignKey(d => d.TripId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Seat)
                      .WithMany(p => p.SeatSegments)
                      .HasForeignKey(d => d.SeatId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Segment)
                      .WithMany(p => p.SeatSegments)
                      .HasForeignKey(d => d.SegmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Booking)
                      .WithMany(p => p.SeatSegments)
                      .HasForeignKey(d => d.BookingId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.TripId, e.SeatId, e.SegmentId }).IsUnique();
            });

            // Booking configurations
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.BookingStatus).HasMaxLength(20).HasDefaultValue("Temporary");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.PaymentStatus).HasMaxLength(20).HasDefaultValue("Pending");
                //entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                //entity.Property(e => e.PaymentTransactionId).HasMaxLength(100);
                //entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.BookingCode).IsUnique();
            });

            // Ticket configurations
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.TicketId);
                entity.Property(e => e.TicketCode).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PassengerName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PassengerIdCard).HasMaxLength(20);
                entity.Property(e => e.PassengerPhone).HasMaxLength(20);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(12,2)").HasDefaultValue(0);
                entity.Property(e => e.FinalPrice).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Valid");
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.TicketCode).IsUnique();
            });

            // Fare configurations
            modelBuilder.Entity<Fare>(entity =>
            {
                entity.HasKey(e => e.FareId);
                entity.Property(e => e.SeatClass).HasMaxLength(20).IsRequired();
                entity.Property(e => e.SeatType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BasePrice).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("VND");

                entity.HasIndex(e => new { e.RouteId, e.SegmentId, e.SeatClass, e.SeatType, e.EffectiveFrom }).IsUnique();
            });

            // Payment configurations
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Amount).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("VND");
                entity.Property(e => e.TransactionId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.GatewayTransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentGateway).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.FailureReason).HasMaxLength(255);
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(12,2)");

                entity.HasIndex(e => e.TransactionId).IsUnique();
            });

            // Additional configurations for other entities...
        }
    }
}