using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class TrainBookingSystemContext : DbContext
{
    public TrainBookingSystemContext()
    {
    }

    public TrainBookingSystemContext(DbContextOptions<TrainBookingSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Carriage> Carriages { get; set; }

    public virtual DbSet<Fare> Fares { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PriceCalculationLog> PriceCalculationLogs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteSegment> RouteSegments { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatSegment> SeatSegments { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketSegment> TicketSegments { get; set; }

    public virtual DbSet<Train> Trains { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripRouteSegment> TripRouteSegments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwBookingDetail> VwBookingDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable(tb =>
                {
                    tb.HasComment("Main booking table supporting both user and guest bookings");
                    tb.HasTrigger("TR_Bookings_UpdatedAt");
                });

            entity.HasIndex(e => e.BookingCode, "IX_Bookings_BookingCode").IsUnique();

            entity.HasIndex(e => e.BookingStatus, "IX_Bookings_BookingStatus");

            entity.HasIndex(e => e.CreatedAt, "IX_Bookings_CreatedAt").IsDescending();

            entity.HasIndex(e => e.ExpirationTime, "IX_Bookings_ExpirationTime").HasFilter("([ExpirationTime] IS NOT NULL AND [BookingStatus]='Temporary')");

            entity.HasIndex(e => new { e.PassengerPhone, e.PassengerEmail }, "IX_Bookings_PassengerInfo");

            entity.HasIndex(e => e.TripId, "IX_Bookings_TripId");

            entity.HasIndex(e => e.UserId, "IX_Bookings_UserId");

            entity.Property(e => e.BookingCode)
                .HasMaxLength(20)
                .HasComment("Unique booking code for identification and lookup");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Temporary");
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.ContactName)
                .HasMaxLength(100)
                .HasComment("Contact information for guest bookings when UserId is null");
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.PassengerEmail).HasMaxLength(100);
            entity.Property(e => e.PassengerIdCard).HasMaxLength(20);
            entity.Property(e => e.PassengerName).HasMaxLength(100);
            entity.Property(e => e.PassengerPhone).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.UserId).HasComment("Nullable for guest bookings. Foreign key to Users table");

            entity.HasOne(d => d.Trip).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Trips");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Bookings_Users");
        });

        modelBuilder.Entity<Carriage>(entity =>
        {
            entity.HasKey(e => e.CarriageId).HasName("PK__Carriage__17FE2DD02745C07A");

            entity.ToTable("Carriage");

            entity.HasIndex(e => new { e.TrainId, e.CarriageNumber }, "UQ_Carriage_Train_Number").IsUnique();

            entity.HasIndex(e => new { e.TrainId, e.Order }, "UQ_Carriage_Train_Order").IsUnique();

            entity.Property(e => e.CarriageNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CarriageType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Train).WithMany(p => p.Carriages)
                .HasForeignKey(d => d.TrainId)
                .HasConstraintName("FK_Carriage_Train");
        });

        modelBuilder.Entity<Fare>(entity =>
        {
            entity.HasKey(e => e.FareId).HasName("PK__Fare__1261FA16E320F637");

            entity.ToTable("Fare");

            entity.HasIndex(e => e.SeatClass, "IX_Fare_Class");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo }, "IX_Fare_EffectiveDate");

            entity.HasIndex(e => e.RouteId, "IX_Fare_Route");

            entity.HasIndex(e => e.SegmentId, "IX_Fare_Segment");

            entity.HasIndex(e => new { e.RouteId, e.SegmentId, e.SeatClass, e.SeatType, e.EffectiveFrom }, "UQ_Fare_Route_Segment_Class_Type_Date").IsUnique();

            entity.Property(e => e.BasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("VND");
            entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");
            entity.Property(e => e.EffectiveTo).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SeatClass)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SeatType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Route).WithMany(p => p.Fares)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fare_Route");

            entity.HasOne(d => d.Segment).WithMany(p => p.Fares)
                .HasForeignKey(d => d.SegmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fare_Segment");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E124909F65F");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.CreatedAt, "IX_Notification_CreatedAt");

            entity.HasIndex(e => e.IsRead, "IX_Notification_IsRead");

            entity.HasIndex(e => e.Type, "IX_Notification_Type");

            entity.HasIndex(e => e.UserId, "IX_Notification_User");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailSent).HasDefaultValue(false);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Normal");
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SmsSent).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A3858E416AE");

            entity.ToTable("Payment");

            entity.HasIndex(e => e.BookingId, "IX_Payment_Booking");

            entity.HasIndex(e => e.PaymentGateway, "IX_Payment_Gateway");

            entity.HasIndex(e => e.Status, "IX_Payment_Status");

            entity.HasIndex(e => e.TransactionId, "IX_Payment_TransactionId");

            entity.HasIndex(e => e.TransactionId, "UQ__Payment__55433A6AB94749FA").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ConfirmedTime).HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("VND");
            entity.Property(e => e.FailureReason).HasMaxLength(255);
            entity.Property(e => e.GatewayTransactionId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PaymentGateway)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PaymentTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.RefundTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PriceCalculationLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__PriceCal__5E5486481C61A8B3");

            entity.ToTable("PriceCalculationLog");

            entity.HasIndex(e => e.BookingId, "IX_PriceLog_Booking");

            entity.HasIndex(e => e.CalculationTime, "IX_PriceLog_CalculationTime");

            entity.HasIndex(e => e.TripId, "IX_PriceLog_Trip");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CalculationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PricingMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SeatClass)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SeatType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Segment).WithMany(p => p.PriceCalculationLogs)
                .HasForeignKey(d => d.SegmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PriceLog_Segment");

            entity.HasOne(d => d.Trip).WithMany(p => p.PriceCalculationLogs)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PriceLog_Trip");

            entity.HasOne(d => d.User).WithMany(p => p.PriceCalculationLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PriceLog_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A0411D81A");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B61600FD7C454").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__Route__80979B4DFDC6C48E");

            entity.ToTable("Route");

            entity.HasIndex(e => e.ArrivalStationId, "IX_Route_ArrivalStation");

            entity.HasIndex(e => e.RouteCode, "IX_Route_Code");

            entity.HasIndex(e => e.DepartureStationId, "IX_Route_DepartureStation");

            entity.HasIndex(e => e.RouteCode, "UQ__Route__FDC345856E96EA5A").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RouteCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RouteName).HasMaxLength(100);
            entity.Property(e => e.TotalDistance).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ArrivalStation).WithMany(p => p.RouteArrivalStations)
                .HasForeignKey(d => d.ArrivalStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Route_ArrivalStation");

            entity.HasOne(d => d.DepartureStation).WithMany(p => p.RouteDepartureStations)
                .HasForeignKey(d => d.DepartureStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Route_DepartureStation");
        });

        modelBuilder.Entity<RouteSegment>(entity =>
        {
            entity.HasKey(e => e.SegmentId).HasName("PK__RouteSeg__C680677B9C803EE6");

            entity.ToTable("RouteSegment");

            entity.HasIndex(e => e.FromStationId, "IX_RouteSegment_FromStation");

            entity.HasIndex(e => new { e.RouteId, e.Order }, "IX_RouteSegment_Order");

            entity.HasIndex(e => e.RouteId, "IX_RouteSegment_Route");

            entity.HasIndex(e => e.ToStationId, "IX_RouteSegment_ToStation");

            entity.HasIndex(e => new { e.RouteId, e.Order }, "UQ_RouteSegment_Route_Order").IsUnique();

            entity.HasIndex(e => new { e.RouteId, e.FromStationId, e.ToStationId }, "UQ_RouteSegment_Route_Stations").IsUnique();

            entity.Property(e => e.Distance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.FromStation).WithMany(p => p.RouteSegmentFromStations)
                .HasForeignKey(d => d.FromStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RouteSegment_FromStation");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteSegments)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK_RouteSegment_Route");

            entity.HasOne(d => d.ToStation).WithMany(p => p.RouteSegmentToStations)
                .HasForeignKey(d => d.ToStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RouteSegment_ToStation");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__Seat__311713F3C3BBD9F7");

            entity.ToTable("Seat");

            entity.HasIndex(e => new { e.CarriageId, e.SeatNumber }, "UQ_Seat_Carriage_Number").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SeatClass)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SeatType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Carriage).WithMany(p => p.Seats)
                .HasForeignKey(d => d.CarriageId)
                .HasConstraintName("FK_Seat_Carriage");
        });

        modelBuilder.Entity<SeatSegment>(entity =>
        {
            entity.HasKey(e => e.SeatSegmentId).HasName("PK__SeatSegm__9F884558C31E25BC");

            entity.ToTable("SeatSegment");

            entity.HasIndex(e => e.BookingId, "IX_SeatSegment_Booking");

            entity.HasIndex(e => e.SeatId, "IX_SeatSegment_Seat");

            entity.HasIndex(e => e.SegmentId, "IX_SeatSegment_Segment");

            entity.HasIndex(e => e.Status, "IX_SeatSegment_Status");

            entity.HasIndex(e => e.TripId, "IX_SeatSegment_Trip");

            entity.HasIndex(e => new { e.TripId, e.SeatId, e.SegmentId }, "UQ_SeatSegment_Trip_Seat_Segment").IsUnique();

            entity.Property(e => e.BookedAt).HasColumnType("datetime");
            entity.Property(e => e.ReservedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Seat).WithMany(p => p.SeatSegments)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatSegment_Seat");

            entity.HasOne(d => d.Segment).WithMany(p => p.SeatSegments)
                .HasForeignKey(d => d.SegmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatSegment_Segment");

            entity.HasOne(d => d.Trip).WithMany(p => p.SeatSegments)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK_SeatSegment_Trip");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PK__Station__E0D8A6BD101859C6");

            entity.ToTable("Station");

            entity.HasIndex(e => e.City, "IX_Station_City");

            entity.HasIndex(e => e.Province, "IX_Station_Province");

            entity.HasIndex(e => e.StationCode, "IX_Station_StationCode");

            entity.HasIndex(e => e.StationCode, "UQ__Station__D38856183F30B3DC").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.Province).HasMaxLength(50);
            entity.Property(e => e.StationCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.StationName).HasMaxLength(100);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC6073664A223");

            entity.ToTable("Ticket");

            entity.HasIndex(e => e.BookingId, "IX_Ticket_Booking");

            entity.HasIndex(e => e.TicketCode, "IX_Ticket_Code");

            entity.HasIndex(e => e.Status, "IX_Ticket_Status");

            entity.HasIndex(e => e.TripId, "IX_Ticket_Trip");

            entity.HasIndex(e => e.UserId, "IX_Ticket_User");

            entity.HasIndex(e => e.TicketCode, "UQ__Ticket__598CF7A31AD93DDD").IsUnique();

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PassengerIdCard)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PassengerName).HasMaxLength(100);
            entity.Property(e => e.PassengerPhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PurchaseTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Valid");
            entity.Property(e => e.TicketCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Trip).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Trip");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_User");
        });

        modelBuilder.Entity<TicketSegment>(entity =>
        {
            entity.HasKey(e => e.TicketSegmentId).HasName("PK__TicketSe__EA257AC5A4C6D04D");

            entity.ToTable("TicketSegment");

            entity.HasIndex(e => new { e.TicketId, e.SegmentId }, "UQ_TicketSegment_Ticket_Segment").IsUnique();

            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.CheckInStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("NotCheckedIn");
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");
            entity.Property(e => e.SegmentPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Seat).WithMany(p => p.TicketSegments)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketSegment_Seat");

            entity.HasOne(d => d.Segment).WithMany(p => p.TicketSegments)
                .HasForeignKey(d => d.SegmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketSegment_Segment");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketSegments)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_TicketSegment_Ticket");
        });

        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(e => e.TrainId).HasName("PK__Train__8ED2723AFBD1DB0C");

            entity.ToTable("Train");

            entity.HasIndex(e => e.TrainNumber, "IX_Train_TrainNumber");

            entity.HasIndex(e => e.TrainType, "IX_Train_TrainType");

            entity.HasIndex(e => e.TrainNumber, "UQ__Train__10C2CD2F0DE83C07").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.TrainName).HasMaxLength(100);
            entity.Property(e => e.TrainNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrainType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PK__Trip__51DC713E7602C160");

            entity.ToTable("Trip");

            entity.HasIndex(e => e.TripCode, "IX_Trip_Code");

            entity.HasIndex(e => e.DepartureTime, "IX_Trip_DepartureTime");

            entity.HasIndex(e => e.RouteId, "IX_Trip_Route");

            entity.HasIndex(e => e.Status, "IX_Trip_Status");

            entity.HasIndex(e => e.TrainId, "IX_Trip_Train");

            entity.HasIndex(e => e.TripCode, "UQ__Trip__4992CD16B4D0ADEE").IsUnique();

            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DelayMinutes).HasDefaultValue(0);
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled");
            entity.Property(e => e.TripCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TripName).HasMaxLength(100);

            entity.HasOne(d => d.Route).WithMany(p => p.Trips)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trip_Route");

            entity.HasOne(d => d.Train).WithMany(p => p.Trips)
                .HasForeignKey(d => d.TrainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trip_Train");
        });

        modelBuilder.Entity<TripRouteSegment>(entity =>
        {
            entity.HasKey(e => e.TripRouteSegmentId).HasName("PK__TripRout__679ECC10A2A3EA7F");

            entity.ToTable("TripRouteSegment");

            entity.HasIndex(e => new { e.TripId, e.Order }, "UQ_TripRouteSegment_Trip_Order").IsUnique();

            entity.HasIndex(e => new { e.TripId, e.RouteSegmentId }, "UQ_TripRouteSegment_Trip_Segment").IsUnique();

            entity.Property(e => e.ActualArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.ActualDepartureTime).HasColumnType("datetime");
            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.DelayMinutes).HasDefaultValue(0);
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");

            entity.HasOne(d => d.RouteSegment).WithMany(p => p.TripRouteSegments)
                .HasForeignKey(d => d.RouteSegmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripRouteSegment_RouteSegment");

            entity.HasOne(d => d.Trip).WithMany(p => p.TripRouteSegments)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK_TripRouteSegment_Trip");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C5C28BCAE");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "IX_User_Email");

            entity.HasIndex(e => e.Phone, "IX_User_Phone");

            entity.HasIndex(e => e.Username, "IX_User_Username");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E445C15E6C").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534AF1C82A4").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A352B1225BA");

            entity.ToTable("UserRole");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRole_User_Role").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRole_User");
        });

        modelBuilder.Entity<VwBookingDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_BookingDetails");

            entity.Property(e => e.ArrivalStation).HasMaxLength(100);
            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.BookingCode).HasMaxLength(20);
            entity.Property(e => e.BookingStatus).HasMaxLength(20);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.DepartureStation).HasMaxLength(100);
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");
            entity.Property(e => e.PassengerEmail).HasMaxLength(100);
            entity.Property(e => e.PassengerName).HasMaxLength(100);
            entity.Property(e => e.PassengerPhone).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.RouteName).HasMaxLength(100);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrainNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TripCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserFullName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
