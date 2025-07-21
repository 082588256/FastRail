-- =====================================================
-- TRAIN BOOKING SYSTEM DATABASE CREATION SCRIPT
-- Created: 2025-06-30
-- Description: Complete database schema and sample data for Vietnamese train booking system
-- =====================================================

USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TrainBookingSystem')
DROP DATABASE TrainBookingSystem;
GO

-- Create database
CREATE DATABASE TrainBookingSystem;
GO

USE TrainBookingSystem;
GO

-- =====================================================
-- TABLE CREATION SECTION
-- =====================================================

-- 1. Create Role table
CREATE TABLE Role (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1
);

-- 2. Create User table
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    PasswordHash VARCHAR(255) NOT NULL,
    DateOfBirth DATE,
    Gender VARCHAR(10),
    Address NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CK_User_Gender CHECK (Gender IN ('Male', 'Female', 'Other')),
    CONSTRAINT CK_User_Phone CHECK (Phone LIKE '[0-9]%'),
    CONSTRAINT CK_User_Email CHECK (Email LIKE '%@%.%')
);

-- 3. Create UserRole table
CREATE TABLE UserRole (
    UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES Role(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRole_User_Role UNIQUE (UserId, RoleId)
);

-- 4. Create Station table
CREATE TABLE Station (
    StationId INT IDENTITY(1,1) PRIMARY KEY,
    StationName NVARCHAR(100) NOT NULL,
    StationCode VARCHAR(10) NOT NULL UNIQUE,
    City NVARCHAR(50) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    Address NVARCHAR(255),
    Latitude DECIMAL(10, 8),
    Longitude DECIMAL(11, 8),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CK_Station_StationCode CHECK (LEN(StationCode) >= 2),
    CONSTRAINT CK_Station_Latitude CHECK (Latitude BETWEEN -90 AND 90),
    CONSTRAINT CK_Station_Longitude CHECK (Longitude BETWEEN -180 AND 180)
);

-- 5. Create Route table
CREATE TABLE Route (
    RouteId INT IDENTITY(1,1) PRIMARY KEY,
    RouteName NVARCHAR(100) NOT NULL,
    RouteCode VARCHAR(20) NOT NULL UNIQUE,
    DepartureStationId INT NOT NULL,
    ArrivalStationId INT NOT NULL,
    TotalDistance DECIMAL(10,2),
    EstimatedDuration INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Route_DepartureStation FOREIGN KEY (DepartureStationId) REFERENCES Station(StationId),
    CONSTRAINT FK_Route_ArrivalStation FOREIGN KEY (ArrivalStationId) REFERENCES Station(StationId),
    CONSTRAINT CK_Route_DifferentStations CHECK (DepartureStationId != ArrivalStationId),
    CONSTRAINT CK_Route_Distance CHECK (TotalDistance > 0),
    CONSTRAINT CK_Route_Duration CHECK (EstimatedDuration > 0)
);

-- 6. Create RouteSegment table
CREATE TABLE RouteSegment (
    SegmentId INT IDENTITY(1,1) PRIMARY KEY,
    RouteId INT NOT NULL,
    FromStationId INT NOT NULL,
    ToStationId INT NOT NULL,
    [Order] INT NOT NULL,
    Distance DECIMAL(10,2) NOT NULL,
    EstimatedDuration INT NOT NULL,
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_RouteSegment_Route FOREIGN KEY (RouteId) REFERENCES Route(RouteId) ON DELETE CASCADE,
    CONSTRAINT FK_RouteSegment_FromStation FOREIGN KEY (FromStationId) REFERENCES Station(StationId),
    CONSTRAINT FK_RouteSegment_ToStation FOREIGN KEY (ToStationId) REFERENCES Station(StationId),
    CONSTRAINT CK_RouteSegment_DifferentStations CHECK (FromStationId != ToStationId),
    CONSTRAINT CK_RouteSegment_Order CHECK ([Order] >= 1),
    CONSTRAINT CK_RouteSegment_Distance CHECK (Distance > 0),
    CONSTRAINT CK_RouteSegment_Duration CHECK (EstimatedDuration > 0),
    CONSTRAINT UQ_RouteSegment_Route_Order UNIQUE (RouteId, [Order]),
    CONSTRAINT UQ_RouteSegment_Route_Stations UNIQUE (RouteId, FromStationId, ToStationId)
);

-- 7. Create Train table
CREATE TABLE Train (
    TrainId INT IDENTITY(1,1) PRIMARY KEY,
    TrainNumber VARCHAR(20) NOT NULL UNIQUE,
    TrainName NVARCHAR(100),
    TrainType VARCHAR(50) NOT NULL,
    TotalCarriages INT NOT NULL,
    MaxSpeed INT,
    Manufacturer NVARCHAR(100),
    YearOfManufacture INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CK_Train_TotalCarriages CHECK (TotalCarriages BETWEEN 1 AND 20),
    CONSTRAINT CK_Train_MaxSpeed CHECK (MaxSpeed > 0),
    CONSTRAINT CK_Train_YearOfManufacture CHECK (YearOfManufacture BETWEEN 1950 AND YEAR(GETDATE()))
);

-- 8. Create Carriage table
CREATE TABLE Carriage (
    CarriageId INT IDENTITY(1,1) PRIMARY KEY,
    TrainId INT NOT NULL,
    CarriageNumber VARCHAR(10) NOT NULL,
    CarriageType VARCHAR(50) NOT NULL,
    TotalSeats INT NOT NULL,
    [Order] INT NOT NULL,
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_Carriage_Train FOREIGN KEY (TrainId) REFERENCES Train(TrainId) ON DELETE CASCADE,
    CONSTRAINT CK_Carriage_TotalSeats CHECK (TotalSeats BETWEEN 1 AND 100),
    CONSTRAINT CK_Carriage_Order CHECK ([Order] >= 1),
    CONSTRAINT UQ_Carriage_Train_Number UNIQUE (TrainId, CarriageNumber),
    CONSTRAINT UQ_Carriage_Train_Order UNIQUE (TrainId, [Order])
);

-- 9. Create Seat table
CREATE TABLE Seat (
    SeatId INT IDENTITY(1,1) PRIMARY KEY,
    CarriageId INT NOT NULL,
    SeatNumber VARCHAR(10) NOT NULL,
    SeatType VARCHAR(50) NOT NULL,
    SeatClass VARCHAR(20) NOT NULL,
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_Seat_Carriage FOREIGN KEY (CarriageId) REFERENCES Carriage(CarriageId) ON DELETE CASCADE,
    CONSTRAINT UQ_Seat_Carriage_Number UNIQUE (CarriageId, SeatNumber),
    CONSTRAINT CK_Seat_Type CHECK (SeatType IN ('Window', 'Aisle', 'Upper', 'Lower', 'Middle')),
    CONSTRAINT CK_Seat_Class CHECK (SeatClass IN ('Economy', 'Business', 'VIP'))
);

-- 10. Create Trip table
CREATE TABLE Trip (
    TripId INT IDENTITY(1,1) PRIMARY KEY,
    TrainId INT NOT NULL,
    RouteId INT NOT NULL,
    TripCode VARCHAR(50) NOT NULL UNIQUE,
    TripName NVARCHAR(100),
    DepartureTime DATETIME NOT NULL,
    ArrivalTime DATETIME NOT NULL,
    Status VARCHAR(20) DEFAULT 'Scheduled',
    DelayMinutes INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Trip_Train FOREIGN KEY (TrainId) REFERENCES Train(TrainId),
    CONSTRAINT FK_Trip_Route FOREIGN KEY (RouteId) REFERENCES Route(RouteId),
    CONSTRAINT CK_Trip_DepartureBeforeArrival CHECK (DepartureTime < ArrivalTime),
    CONSTRAINT CK_Trip_Status CHECK (Status IN ('Scheduled', 'Active', 'Delayed', 'Cancelled', 'Completed')),
    CONSTRAINT CK_Trip_DelayMinutes CHECK (DelayMinutes >= 0)
);

-- 11. Create TripRouteSegment table
CREATE TABLE TripRouteSegment (
    TripRouteSegmentId INT IDENTITY(1,1) PRIMARY KEY,
    TripId INT NOT NULL,
    RouteSegmentId INT NOT NULL,
    DepartureTime DATETIME NOT NULL,
    ArrivalTime DATETIME,
    [Order] INT NOT NULL,
    ActualDepartureTime DATETIME,
    ActualArrivalTime DATETIME,
    DelayMinutes INT DEFAULT 0,
    
    CONSTRAINT FK_TripRouteSegment_Trip FOREIGN KEY (TripId) REFERENCES Trip(TripId) ON DELETE CASCADE,
    CONSTRAINT FK_TripRouteSegment_RouteSegment FOREIGN KEY (RouteSegmentId) REFERENCES RouteSegment(SegmentId),
    CONSTRAINT CK_TripRouteSegment_DepartureBeforeArrival CHECK (DepartureTime <= ArrivalTime),
    CONSTRAINT CK_TripRouteSegment_Order CHECK ([Order] >= 1),
    CONSTRAINT CK_TripRouteSegment_DelayMinutes CHECK (DelayMinutes >= 0),
    CONSTRAINT UQ_TripRouteSegment_Trip_Segment UNIQUE (TripId, RouteSegmentId),
    CONSTRAINT UQ_TripRouteSegment_Trip_Order UNIQUE (TripId, [Order])
);

-- 12. Create Booking table
CREATE TABLE Booking (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    TripId INT NOT NULL,
    BookingCode VARCHAR(20) NOT NULL UNIQUE,
    BookingStatus VARCHAR(20) DEFAULT 'Temporary',
    ContactEmail NVARCHAR(255) NULL,
    ContactName NVARCHAR(255) NULL,
    ContactPhone NVARCHAR(255) NULL,
    ExpirationTime DATETIME,
    PaymentTransactionId VARCHAR(100),
    PaymentMethod VARCHAR(50),
    PaymentStatus VARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    ConfirmedAt DATETIME,
    CancelledAt DATETIME,
    CONSTRAINT FK_Booking_User FOREIGN KEY (UserId) REFERENCES [User](UserId),
    CONSTRAINT FK_Booking_Trip FOREIGN KEY (TripId) REFERENCES Trip(TripId),
    CONSTRAINT CK_Booking_Status CHECK (BookingStatus IN ('Temporary', 'Confirmed', 'Expired', 'Cancelled')),
    CONSTRAINT CK_Booking_PaymentStatus CHECK (PaymentStatus IN ('Pending', 'Completed', 'Failed', 'Refunded'))
);

-- 13. Create SeatSegment table
CREATE TABLE SeatSegment (
    SeatSegmentId INT IDENTITY(1,1) PRIMARY KEY,
    TripId INT NOT NULL,
    SeatId INT NOT NULL,
    SegmentId INT NOT NULL,
    BookingId INT NULL,
    Status VARCHAR(20) DEFAULT 'Available',
    ReservedAt DATETIME,
    BookedAt DATETIME,
    
    CONSTRAINT FK_SeatSegment_Trip FOREIGN KEY (TripId) REFERENCES Trip(TripId) ON DELETE CASCADE,
    CONSTRAINT FK_SeatSegment_Seat FOREIGN KEY (SeatId) REFERENCES Seat(SeatId),
    CONSTRAINT FK_SeatSegment_Segment FOREIGN KEY (SegmentId) REFERENCES RouteSegment(SegmentId),
    CONSTRAINT FK_SeatSegment_Booking FOREIGN KEY (BookingId) REFERENCES Booking(BookingId),
    CONSTRAINT CK_SeatSegment_Status CHECK (Status IN ('Available', 'TemporaryReserved', 'Booked', 'Blocked')),
    CONSTRAINT UQ_SeatSegment_Trip_Seat_Segment UNIQUE (TripId, SeatId, SegmentId)
);

-- 14. Create Ticket table
CREATE TABLE Ticket (
    TicketId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    UserId INT NULL,
    TripId INT NOT NULL,
    TicketCode VARCHAR(50) NOT NULL UNIQUE,
    PassengerName NVARCHAR(100) NOT NULL,
    PassengerIdCard VARCHAR(20),
    PassengerPhone VARCHAR(20),
    TotalPrice DECIMAL(12,2) NOT NULL,
    DiscountAmount DECIMAL(12,2) DEFAULT 0,
    FinalPrice DECIMAL(12,2) NOT NULL,
    PurchaseTime DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'Valid',
    CheckInTime DATETIME,
    Notes NVARCHAR(500),
    
    CONSTRAINT FK_Ticket_Booking FOREIGN KEY (BookingId) REFERENCES Booking(BookingId),
    CONSTRAINT FK_Ticket_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE SET NULL,
    CONSTRAINT FK_Ticket_Trip FOREIGN KEY (TripId) REFERENCES Trip(TripId),
    CONSTRAINT CK_Ticket_Status CHECK (Status IN ('Valid', 'Used', 'Cancelled', 'Refunded', 'Expired')),
    CONSTRAINT CK_Ticket_TotalPrice CHECK (TotalPrice > 0),
    CONSTRAINT CK_Ticket_DiscountAmount CHECK (DiscountAmount >= 0),
    CONSTRAINT CK_Ticket_FinalPrice CHECK (FinalPrice > 0)
);

-- 15. Create TicketSegment table
CREATE TABLE TicketSegment (
    TicketSegmentId INT IDENTITY(1,1) PRIMARY KEY,
    TicketId INT NOT NULL,
    SegmentId INT NOT NULL,
    SeatId INT NOT NULL,
    SegmentPrice DECIMAL(10,2) NOT NULL,
    DepartureTime DATETIME NOT NULL,
    ArrivalTime DATETIME,
    CheckInStatus VARCHAR(20) DEFAULT 'NotCheckedIn',
    
    CONSTRAINT FK_TicketSegment_Ticket FOREIGN KEY (TicketId) REFERENCES Ticket(TicketId) ON DELETE CASCADE,
    CONSTRAINT FK_TicketSegment_Segment FOREIGN KEY (SegmentId) REFERENCES RouteSegment(SegmentId),
    CONSTRAINT FK_TicketSegment_Seat FOREIGN KEY (SeatId) REFERENCES Seat(SeatId),
    CONSTRAINT CK_TicketSegment_Price CHECK (SegmentPrice > 0),
    CONSTRAINT CK_TicketSegment_CheckInStatus CHECK (CheckInStatus IN ('NotCheckedIn', 'CheckedIn', 'Boarded', 'Completed')),
    CONSTRAINT UQ_TicketSegment_Ticket_Segment UNIQUE (TicketId, SegmentId)
);

-- 16. Create Fare table
CREATE TABLE Fare (
    FareId INT IDENTITY(1,1) PRIMARY KEY,
    RouteId INT NOT NULL,
    SegmentId INT NOT NULL,
    SeatClass VARCHAR(20) NOT NULL,
    SeatType VARCHAR(50) NOT NULL,
    BasePrice DECIMAL(10,2) NOT NULL,
    Currency VARCHAR(3) DEFAULT 'VND',
    EffectiveFrom DATETIME NOT NULL,
    EffectiveTo DATETIME,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Fare_Route FOREIGN KEY (RouteId) REFERENCES Route(RouteId),
    CONSTRAINT FK_Fare_Segment FOREIGN KEY (SegmentId) REFERENCES RouteSegment(SegmentId),
    CONSTRAINT CK_Fare_BasePrice CHECK (BasePrice > 0),
    CONSTRAINT CK_Fare_SeatClass CHECK (SeatClass IN ('Economy', 'Business', 'VIP')),
    CONSTRAINT CK_Fare_SeatType CHECK (SeatType IN ('Window', 'Aisle', 'Upper', 'Lower', 'Middle')),
    CONSTRAINT CK_Fare_EffectivePeriod CHECK (EffectiveFrom < EffectiveTo OR EffectiveTo IS NULL),
    CONSTRAINT UQ_Fare_Route_Segment_Class_Type_Date UNIQUE (RouteId, SegmentId, SeatClass, SeatType, EffectiveFrom)
);

-- 17. Create Payment table
CREATE TABLE Payment (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    Currency VARCHAR(3) DEFAULT 'VND',
    TransactionId VARCHAR(100) NOT NULL UNIQUE,
    GatewayTransactionId VARCHAR(100),
    PaymentGateway VARCHAR(50),
    Status VARCHAR(20) DEFAULT 'Pending',
    PaymentTime DATETIME DEFAULT GETDATE(),
    ConfirmedTime DATETIME,
    FailureReason NVARCHAR(255),
    RefundAmount DECIMAL(12,2),
    RefundTime DATETIME,
    
    CONSTRAINT FK_Payment_Booking FOREIGN KEY (BookingId) REFERENCES Booking(BookingId),
    CONSTRAINT CK_Payment_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Payment_Status CHECK (Status IN ('Pending', 'Completed', 'Failed', 'Cancelled', 'Refunded')),
    CONSTRAINT CK_Payment_RefundAmount CHECK (RefundAmount >= 0 AND RefundAmount <= Amount)
);

-- 18. Create Notification table
CREATE TABLE Notification (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    IsRead BIT DEFAULT 0,
    Priority VARCHAR(20) DEFAULT 'Normal',
    RelatedEntityType VARCHAR(50),
    RelatedEntityId INT,
    EmailSent BIT DEFAULT 0,
    SmsSent BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    ReadAt DATETIME,
    
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE,
    CONSTRAINT CK_Notification_Priority CHECK (Priority IN ('Low', 'Normal', 'High', 'Critical'))
);

-- 19. Create PriceCalculationLog table
CREATE TABLE PriceCalculationLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT,
    TripId INT NOT NULL,
    SegmentId INT NOT NULL,
    SeatClass VARCHAR(20) NOT NULL,
    SeatType VARCHAR(50) NOT NULL,
    BasePrice DECIMAL(10,2) NOT NULL,
    FinalPrice DECIMAL(10,2) NOT NULL,
    PricingMethod VARCHAR(50) NOT NULL,
    PricingFactors NVARCHAR(MAX),
    CalculationTime DATETIME DEFAULT GETDATE(),
    UserId INT,
    
    CONSTRAINT FK_PriceLog_Booking FOREIGN KEY (BookingId) REFERENCES Booking(BookingId),
    CONSTRAINT FK_PriceLog_Trip FOREIGN KEY (TripId) REFERENCES Trip(TripId),
    CONSTRAINT FK_PriceLog_Segment FOREIGN KEY (SegmentId) REFERENCES RouteSegment(SegmentId),
    CONSTRAINT FK_PriceLog_User FOREIGN KEY (UserId) REFERENCES [User](UserId),
    CONSTRAINT CK_PriceLog_BasePrice CHECK (BasePrice > 0),
    CONSTRAINT CK_PriceLog_FinalPrice CHECK (FinalPrice > 0)
);

-- =====================================================
-- CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- User indexes
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_Username ON [User](Username);
CREATE INDEX IX_User_Phone ON [User](Phone);

-- Station indexes
CREATE INDEX IX_Station_StationCode ON Station(StationCode);
CREATE INDEX IX_Station_City ON Station(City);
CREATE INDEX IX_Station_Province ON Station(Province);

-- Route indexes
CREATE INDEX IX_Route_DepartureStation ON Route(DepartureStationId);
CREATE INDEX IX_Route_ArrivalStation ON Route(ArrivalStationId);
CREATE INDEX IX_Route_Code ON Route(RouteCode);

-- RouteSegment indexes
CREATE INDEX IX_RouteSegment_Route ON RouteSegment(RouteId);
CREATE INDEX IX_RouteSegment_FromStation ON RouteSegment(FromStationId);
CREATE INDEX IX_RouteSegment_ToStation ON RouteSegment(ToStationId);
CREATE INDEX IX_RouteSegment_Order ON RouteSegment(RouteId, [Order]);

-- Train indexes
CREATE INDEX IX_Train_TrainNumber ON Train(TrainNumber);
CREATE INDEX IX_Train_TrainType ON Train(TrainType);

-- Trip indexes
CREATE INDEX IX_Trip_DepartureTime ON Trip(DepartureTime);
CREATE INDEX IX_Trip_Route ON Trip(RouteId);
CREATE INDEX IX_Trip_Train ON Trip(TrainId);
CREATE INDEX IX_Trip_Status ON Trip(Status);
CREATE INDEX IX_Trip_Code ON Trip(TripCode);

-- Booking indexes
CREATE INDEX IX_Booking_User ON Booking(UserId);
CREATE INDEX IX_Booking_Trip ON Booking(TripId);
CREATE INDEX IX_Booking_Status ON Booking(BookingStatus);
CREATE INDEX IX_Booking_Code ON Booking(BookingCode);
CREATE INDEX IX_Booking_ExpirationTime ON Booking(ExpirationTime);

-- SeatSegment indexes
CREATE INDEX IX_SeatSegment_Trip ON SeatSegment(TripId);
CREATE INDEX IX_SeatSegment_Seat ON SeatSegment(SeatId);
CREATE INDEX IX_SeatSegment_Segment ON SeatSegment(SegmentId);
CREATE INDEX IX_SeatSegment_Status ON SeatSegment(Status);
CREATE INDEX IX_SeatSegment_Booking ON SeatSegment(BookingId);

-- Ticket indexes
CREATE INDEX IX_Ticket_Booking ON Ticket(BookingId);
CREATE INDEX IX_Ticket_User ON Ticket(UserId);
CREATE INDEX IX_Ticket_Trip ON Ticket(TripId);
CREATE INDEX IX_Ticket_Code ON Ticket(TicketCode);
CREATE INDEX IX_Ticket_Status ON Ticket(Status);

-- Fare indexes
CREATE INDEX IX_Fare_Route ON Fare(RouteId);
CREATE INDEX IX_Fare_Segment ON Fare(SegmentId);
CREATE INDEX IX_Fare_Class ON Fare(SeatClass);
CREATE INDEX IX_Fare_EffectiveDate ON Fare(EffectiveFrom, EffectiveTo);

-- Payment indexes
CREATE INDEX IX_Payment_Booking ON Payment(BookingId);
CREATE INDEX IX_Payment_TransactionId ON Payment(TransactionId);
CREATE INDEX IX_Payment_Status ON Payment(Status);
CREATE INDEX IX_Payment_Gateway ON Payment(PaymentGateway);

-- Notification indexes
CREATE INDEX IX_Notification_User ON Notification(UserId);
CREATE INDEX IX_Notification_Type ON Notification(Type);
CREATE INDEX IX_Notification_IsRead ON Notification(IsRead);
CREATE INDEX IX_Notification_CreatedAt ON Notification(CreatedAt);

-- PriceCalculationLog indexes
CREATE INDEX IX_PriceLog_Booking ON PriceCalculationLog(BookingId);
CREATE INDEX IX_PriceLog_Trip ON PriceCalculationLog(TripId);
CREATE INDEX IX_PriceLog_CalculationTime ON PriceCalculationLog(CalculationTime);

-- =====================================================
-- SAMPLE DATA INSERTION SECTION
-- =====================================================

-- Insert default roles
INSERT INTO Role (RoleName, Description) VALUES 
('Customer', N'Khách hàng đặt vé'),
('Admin', N'Quản trị viên hệ thống'),
('Staff', N'Nhân viên bán vé'),
('Manager', N'Quản lý ga tàu'),
('Driver', N'Lái tàu'),
('Conductor', N'Tiếp viên tàu'),
('Maintenance', N'Bảo trì tàu'),
('Security', N'Bảo vệ'),
('Accountant', N'Kế toán'),
('Support', N'Hỗ trợ khách hàng'),
('Operator', N'Điều hành tàu'),
('Inspector', N'Thanh tra'),
('Cleaner', N'Vệ sinh'),
('Technician', N'Kỹ thuật viên'),
('Supervisor', N'Giám sát viên');

-- Insert sample users
INSERT INTO [User] (Username, Email, FullName, Phone, PasswordHash, DateOfBirth, Gender, Address) VALUES
('nguyenvana', 'nguyenvana@gmail.com', N'Nguyễn Văn A', '0901234567', 'hash123456789abcdef', '1990-05-15', 'Male', N'123 Nguyễn Huệ, Quận 1, TP.HCM'),
('tranthib', 'tranthib@gmail.com', N'Trần Thị B', '0912345678', 'hash234567890bcdefg', '1985-08-22', 'Female', N'456 Lê Lợi, Hà Nội'),
('lequangc', 'lequangc@yahoo.com', N'Lê Quang C', '0923456789', 'hash345678901cdefgh', '1992-12-10', 'Male', N'789 Trần Hưng Đạo, Đà Nẵng'),
('phamthid', 'phamthid@hotmail.com', N'Phạm Thị D', '0934567890', 'hash456789012defghi', '1988-03-07', 'Female', N'321 Võ Thị Sáu, Biên Hòa'),
('hoangvane', 'hoangvane@gmail.com', N'Hoàng Văn E', '0945678901', 'hash567890123efghij', '1995-11-18', 'Male', N'654 Nguyễn Trãi, Huế'),
('vuthif', 'vuthif@gmail.com', N'Vũ Thị F', '0956789012', 'hash678901234fghijk', '1987-07-25', 'Female', N'987 Lý Thường Kiệt, Cần Thơ'),
('dangvang', 'dangvang@yahoo.com', N'Đặng Văn G', '0967890123', 'hash789012345ghijkl', '1993-01-30', 'Male', N'147 Hai Bà Trưng, Vũng Tàu'),
('buitrh', 'buitrh@gmail.com', N'Bùi Trí H', '0978901234', 'hash890123456hijklm', '1991-09-14', 'Male', N'258 Điện Biên Phủ, Nha Trang'),
('dothii', 'dothii@hotmail.com', N'Đỗ Thị I', '0989012345', 'hash901234567ijklmn', '1986-04-12', 'Female', N'369 Nguyễn Văn Cừ, Long An'),
('maiqueenj', 'maiqueenj@gmail.com', N'Mai Quỳnh J', '0990123456', 'hash012345678jklmno', '1994-06-08', 'Female', N'741 Lê Duẩn, Quảng Nam'),
('admin01', 'admin@trainbooking.vn', N'Quản trị viên', '0901111111', 'adminhashabcdef123', '1980-01-01', 'Male', N'Văn phòng Tổng công ty Đường sắt VN'),
('staff01', 'staff01@trainbooking.vn', N'Nhân viên bán vé', '0902222222', 'staffhash123abc456', '1985-02-15', 'Female', N'Ga Hà Nội'),
('manager01', 'manager01@trainbooking.vn', N'Quản lý ga', '0903333333', 'managerhash456def789', '1975-12-20', 'Male', N'Ga Sài Gòn'),
('driver01', 'driver01@trainbooking.vn', N'Lái tàu SE1', '0904444444', 'driverhash789ghi012', '1982-08-10', 'Male', N'Depot Hà Nội'),
('conductor01', 'conductor01@trainbooking.vn', N'Tiếp viên tàu', '0905555555', 'conductorhash012jkl345', '1990-03-25', 'Female', N'Tàu SE3');

-- Insert UserRole relationships
INSERT INTO UserRole (UserId, RoleId) VALUES
(1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (8, 1), (9, 1), (10, 1), -- Customers
(11, 2), (12, 3), (13, 4), (14, 5), (15, 6); -- Admin, Staff, Manager, Driver, Conductor

-- Insert stations (Major Vietnamese railway stations)
INSERT INTO Station (StationName, StationCode, City, Province, Address, Latitude, Longitude) VALUES
(N'Ga Hà Nội', 'HN', N'Hà Nội', N'Hà Nội', N'120 Lê Duẩn, Hoàn Kiếm, Hà Nội', 21.0245, 105.8412),
(N'Ga Sài Gòn', 'SG', N'TP.HCM', N'TP.HCM', N'1 Nguyễn Thông, Quận 3, TP.HCM', 10.7823, 106.6775),
(N'Ga Vinh', 'VI', N'Vinh', N'Nghệ An', N'Đường Lê Lợi, TP.Vinh, Nghệ An', 18.6793, 105.6811),
(N'Ga Huế', 'HU', N'Huế', N'Thừa Thiên Huế', N'Đường Bùi Thị Xuân, TP.Huế', 16.4637, 107.5909),
(N'Ga Đà Nẵng', 'DN', N'Đà Nẵng', N'Đà Nẵng', N'202 Hai Phòng, Thanh Khê, Đà Nẵng', 16.0471, 108.2068),
(N'Ga Nha Trang', 'NT', N'Nha Trang', N'Khánh Hòa', N'26 Thái Nguyên, TP.Nha Trang', 12.2585, 109.1925),
(N'Ga Biên Hòa', 'BH', N'Biên Hòa', N'Đồng Nai', N'Đường Võ Thị Sáu, TP.Biên Hòa', 10.9500, 106.8200),
(N'Ga Dĩ An', 'DA', N'Dĩ An', N'Bình Dương', N'Đường Mỹ Phước Tân Vạn, Dĩ An', 10.9000, 106.7500),
(N'Ga Đông Hà', 'DH', N'Đông Hà', N'Quảng Trị', N'Đường Trần Phú, TP.Đông Hà', 16.8163, 107.1004),
(N'Ga Quảng Ngãi', 'QN', N'Quảng Ngãi', N'Quảng Ngãi', N'Đường Quang Trung, TP.Quảng Ngãi', 15.1214, 108.7921),
(N'Ga Tuy Hòa', 'TH', N'Tuy Hòa', N'Phú Yên', N'Đường Nguyễn Tất Thành, TP.Tuy Hòa', 13.0955, 109.2957),
(N'Ga Diệu Trì', 'DT', N'Diệu Trì', N'Phú Yên', N'Xã Diệu Trì, Tuy Hòa, Phú Yên', 13.1500, 109.3000),
(N'Ga Muống Mán', 'MM', N'Cam Ranh', N'Khánh Hòa', N'Tp.Cam Ranh, Khánh Hòa', 11.9200, 109.1500),
(N'Ga Thạp Chàm', 'TC', N'Phan Rang', N'Ninh Thuận', N'TP.Phan Rang-Tháp Chàm', 11.5449, 108.9829),
(N'Ga Phan Thiết', 'PT', N'Phan Thiết', N'Bình Thuận', N'Đường Nguyễn Tất Thành, TP.Phan Thiết', 10.9287, 108.1016);

-- Insert main routes
INSERT INTO Route (RouteName, RouteCode, DepartureStationId, ArrivalStationId, TotalDistance, EstimatedDuration) VALUES
(N'Tuyến Thống Nhất (Hà Nội - TP.HCM)', 'TN-HN-SG', 1, 2, 1726.0, 1920), -- 32 hours
(N'Tuyến Hà Nội - Vinh', 'HN-VI', 1, 3, 319.0, 360), -- 6 hours
(N'Tuyến Hà Nội - Huế', 'HN-HU', 1, 4, 688.0, 720), -- 12 hours
(N'Tuyến Hà Nội - Đà Nẵng', 'HN-DN', 1, 5, 791.0, 840), -- 14 hours
(N'Tuyến TP.HCM - Nha Trang', 'SG-NT', 2, 6, 411.0, 480), -- 8 hours
(N'Tuyến Vinh - Huế', 'VI-HU', 3, 4, 369.0, 360), -- 6 hours
(N'Tuyến Huế - Đà Nẵng', 'HU-DN', 4, 5, 103.0, 120), -- 2 hours
(N'Tuyến Đà Nẵng - Nha Trang', 'DN-NT', 5, 6, 540.0, 600), -- 10 hours
(N'Tuyến TP.HCM - Biên Hòa', 'SG-BH', 2, 7, 32.0, 45), -- 45 minutes
(N'Tuyến Nha Trang - Phan Thiết', 'NT-PT', 6, 15, 340.0, 400), -- 6.5 hours
(N'Tuyến Đà Nẵng - Huế', 'DN-HU', 5, 4, 103.0, 120), -- 2 hours
(N'Tuyến Vinh - Đà Nẵng', 'VI-DN', 3, 5, 472.0, 480), -- 8 hours
(N'Tuyến Huế - Nha Trang', 'HU-NT', 4, 6, 643.0, 720), -- 12 hours
(N'Tuyến Quảng Ngãi - Nha Trang', 'QN-NT', 10, 6, 340.0, 360), -- 6 hours
(N'Tuyến Tuy Hòa - Nha Trang', 'TH-NT', 11, 6, 120.0, 150); -- 2.5 hours

-- Insert route segments for main route (Hà Nội - TP.HCM)
INSERT INTO RouteSegment (RouteId, FromStationId, ToStationId, [Order], Distance, EstimatedDuration) VALUES
-- Route 1: Hà Nội - TP.HCM
(1, 1, 3, 1, 319.0, 360), -- Hà Nội → Vinh
(1, 3, 4, 2, 369.0, 360), -- Vinh → Huế
(1, 4, 5, 3, 103.0, 120), -- Huế → Đà Nẵng
(1, 5, 10, 4, 131.0, 150), -- Đà Nẵng → Quảng Ngãi
(1, 10, 11, 5, 209.0, 240), -- Quảng Ngãi → Tuy Hòa
(1, 11, 6, 6, 120.0, 150), -- Tuy Hòa → Nha Trang
(1, 6, 14, 7, 105.0, 120), -- Nha Trang → Thạp Chàm
(1, 14, 15, 8, 185.0, 210), -- Thạp Chàm → Phan Thiết
(1, 15, 7, 9, 200.0, 240), -- Phan Thiết → Biên Hòa
(1, 7, 2, 10, 85.0, 90), -- Biên Hòa → TP.HCM

-- Route 2: Hà Nội - Vinh
(2, 1, 3, 1, 319.0, 360),

-- Route 3: Hà Nội - Huế  
(3, 1, 3, 1, 319.0, 360),
(3, 3, 4, 2, 369.0, 360),

-- Route 4: Hà Nội - Đà Nẵng
(4, 1, 3, 1, 319.0, 360),
(4, 3, 4, 2, 369.0, 360), 
(4, 4, 5, 3, 103.0, 120),

-- Route 5: TP.HCM - Nha Trang
(5, 2, 7, 1, 85.0, 90),
(5, 7, 15, 2, 200.0, 240),
(5, 15, 6, 3, 126.0, 150);

-- Insert trains
INSERT INTO Train (TrainNumber, TrainName, TrainType, TotalCarriages, MaxSpeed, Manufacturer, YearOfManufacture) VALUES
('SE1', N'Thống Nhất', 'SE', 16, 120, N'CNR Dalian', 2010),
('SE2', N'Thống Nhất', 'SE', 16, 120, N'CNR Dalian', 2010),
('SE3', N'Thống Nhất', 'SE', 16, 120, N'CNR Dalian', 2011),
('SE4', N'Thống Nhất', 'SE', 16, 120, N'CNR Dalian', 2011),
('SE5', N'Thống Nhất', 'SE', 14, 120, N'CNR Dalian', 2012),
('SE6', N'Thống Nhất', 'SE', 14, 120, N'CNR Dalian', 2012),
('SE7', N'Thống Nhất', 'SE', 12, 120, N'CNR Dalian', 2013),
('SE8', N'Thống Nhất', 'SE', 12, 120, N'CNR Dalian', 2013),
('TN1', N'Tàu Nhanh', 'TN', 10, 100, N'DEMU Vietnam', 2015),
('TN2', N'Tàu Nhanh', 'TN', 10, 100, N'DEMU Vietnam', 2015),
('SPT1', N'Sài Gòn - Phan Thiết', 'SPT', 8, 90, N'DEMU Vietnam', 2016),
('SPT2', N'Sài Gòn - Phan Thiết', 'SPT', 8, 90, N'DEMU Vietnam', 2016),
('SNT1', N'Sài Gòn - Nha Trang', 'SNT', 12, 110, N'CNR Dalian', 2014),
('SNT2', N'Sài Gòn - Nha Trang', 'SNT', 12, 110, N'CNR Dalian', 2014),
('LP1', N'Lào Cai - Hà Nội', 'LP', 6, 80, N'DEMU Vietnam', 2017);

-- Insert carriages for SE1 train (example)
INSERT INTO Carriage (TrainId, CarriageNumber, CarriageType, TotalSeats, [Order]) VALUES
-- SE1 Train carriages
(1, 'T1', 'HardSeat', 64, 1),
(1, 'T2', 'HardSeat', 64, 2),
(1, 'T3', 'SoftSeat', 64, 3),
(1, 'T4', 'SoftSeat', 64, 4),
(1, 'T5', 'Sleeper', 42, 5),
(1, 'T6', 'Sleeper', 42, 6),
(1, 'T7', 'VIP', 28, 7),

-- SE2 Train carriages  
(2, 'T1', 'HardSeat', 64, 1),
(2, 'T2', 'HardSeat', 64, 2),
(2, 'T3', 'SoftSeat', 64, 3),
(2, 'T4', 'Sleeper', 42, 4),
(2, 'T5', 'Sleeper', 42, 5),
(2, 'T6', 'VIP', 28, 6),

-- TN1 Train carriages
(9, 'T1', 'HardSeat', 80, 1),
(9, 'T2', 'SoftSeat', 64, 2),
(9, 'T3', 'Sleeper', 42, 3);

-- Insert sample seats for carriage T1 of SE1 train
INSERT INTO Seat (CarriageId, SeatNumber, SeatType, SeatClass) VALUES
-- Carriage T1 of SE1 (HardSeat) - 15 seats as sample
(1, '1A', 'Window', 'Economy'),
(1, '1B', 'Aisle', 'Economy'),
(1, '2A', 'Window', 'Economy'),
(1, '2B', 'Aisle', 'Economy'),
(1, '3A', 'Window', 'Economy'),
(1, '3B', 'Aisle', 'Economy'),
(1, '4A', 'Window', 'Economy'),
(1, '4B', 'Aisle', 'Economy'),
(1, '5A', 'Window', 'Economy'),
(1, '5B', 'Aisle', 'Economy'),

-- Carriage T7 of SE1 (VIP) - 5 seats as sample
(7, '1A', 'Window', 'VIP'),
(7, '1B', 'Aisle', 'VIP'),
(7, '2A', 'Window', 'VIP'),
(7, '2B', 'Aisle', 'VIP'),
(7, '3A', 'Window', 'VIP');

-- Insert trips
INSERT INTO Trip (TrainId, RouteId, TripCode, TripName, DepartureTime, ArrivalTime, Status) VALUES
-- SE1 trips on main route
(1, 1, 'SE1-20250701', N'SE1 - Hà Nội đi TP.HCM', '2025-09-01 19:30:00', '2025-09-03 03:30:00', 'Scheduled'),
(1, 1, 'SE1-20250703', N'SE1 - Hà Nội đi TP.HCM', '2025-09-03 19:30:00', '2025-09-05 03:30:00', 'Scheduled'),
(1, 1, 'SE1-20250705', N'SE1 - Hà Nội đi TP.HCM', '2025-09-05 19:30:00', '2025-09-07 03:30:00', 'Scheduled'),

-- SE2 reverse trips  
(2, 1, 'SE2-20250702', N'SE2 - TP.HCM đi Hà Nội', '2025-09-02 19:00:00', '2025-09-04 03:00:00', 'Scheduled'),
(2, 1, 'SE2-20250704', N'SE2 - TP.HCM đi Hà Nội', '2025-09-04 19:00:00', '2025-09-06 03:00:00', 'Scheduled'),
(2, 1, 'SE2-20250706', N'SE2 - TP.HCM đi Hà Nội', '2025-09-06 19:00:00', '2025-09-08 03:00:00', 'Scheduled'),

-- SE3 trips
(3, 1, 'SE3-20250701', N'SE3 - Hà Nội đi TP.HCM', '2025-09-01 22:00:00', '2025-09-03 06:00:00', 'Scheduled'),
(3, 1, 'SE3-20250702', N'SE3 - Hà Nội đi TP.HCM', '2025-07-02 22:00:00', '2025-09-04 06:00:00', 'Scheduled'),

-- TN1 short trips
(9, 2, 'TN1-20250701', N'TN1 - Hà Nội đi Vinh', '2025-09-01 06:00:00', '2025-09-01 12:00:00', 'Scheduled'),
(9, 2, 'TN1-20250701B', N'TN1 - Hà Nội đi Vinh', '2025-09-01 14:00:00', '2025-09-01 20:00:00', 'Scheduled'),
(9, 2, 'TN1-20250702', N'TN1 - Hà Nội đi Vinh', '2025-09-02 06:00:00', '2025-09-02 12:00:00', 'Scheduled'),

-- SPT trains
(11, 5, 'SPT1-20250701', N'SPT1 - TP.HCM đi Nha Trang', '2025-09-01 07:30:00', '2025-09-01 15:30:00', 'Scheduled'),
(11, 5, 'SPT1-20250702', N'SPT1 - TP.HCM đi Nha Trang', '2025-09-02 07:30:00', '2025-09-02 15:30:00', 'Scheduled'),

-- SNT trains
(13, 5, 'SNT1-20250701', N'SNT1 - TP.HCM đi Nha Trang', '2025-09-01 22:30:00', '2025-09-02 06:30:00', 'Scheduled'),
(13, 5, 'SNT1-20250702', N'SNT1 - TP.HCM đi Nha Trang', '2025-09-02 22:30:00', '2025-09-03 06:30:00', 'Scheduled');

-- Insert trip route segments for Trip 1 (SE1-20250701)
INSERT INTO TripRouteSegment (TripId, RouteSegmentId, DepartureTime, ArrivalTime, [Order]) VALUES
-- Trip 1: SE1-20250701 segments
(1, 1, '2025-07-01 19:30:00', '2025-07-02 01:30:00', 1), -- Hà Nội → Vinh
(1, 2, '2025-07-02 02:00:00', '2025-07-02 08:00:00', 2), -- Vinh → Huế
(1, 3, '2025-07-02 08:30:00', '2025-07-02 10:30:00', 3), -- Huế → Đà Nẵng
(1, 4, '2025-07-02 11:00:00', '2025-07-02 13:30:00', 4), -- Đà Nẵng → Quảng Ngãi
(1, 5, '2025-07-02 14:00:00', '2025-07-02 18:00:00', 5), -- Quảng Ngãi → Tuy Hòa
(1, 6, '2025-07-02 18:30:00', '2025-07-02 21:00:00', 6), -- Tuy Hòa → Nha Trang
(1, 7, '2025-07-02 21:30:00', '2025-07-02 23:30:00', 7), -- Nha Trang → Thạp Chàm
(1, 8, '2025-07-03 00:00:00', '2025-07-03 03:30:00', 8), -- Thạp Chàm → Phan Thiết
(1, 9, '2025-07-03 04:00:00', '2025-07-03 08:00:00', 9), -- Phan Thiết → Biên Hòa
(1, 10, '2025-07-03 08:30:00', '2025-07-03 10:00:00', 10), -- Biên Hòa → TP.HCM

-- Trip 9: TN1-20250701 segments
(9, 11, '2025-07-01 06:00:00', '2025-07-01 12:00:00', 1), -- Hà Nội → Vinh

-- Trip 12: SPT1-20250701 segments
(12, 16, '2025-07-01 07:30:00', '2025-07-01 09:00:00', 1), -- TP.HCM → Biên Hòa
(12, 17, '2025-07-01 09:30:00', '2025-07-01 13:30:00', 2), -- Biên Hòa → Phan Thiết
(12, 18, '2025-07-01 14:00:00', '2025-07-01 16:30:00', 3); -- Phan Thiết → Nha Trang

-- Insert sample bookings
INSERT INTO Booking (UserId, TripId, BookingCode, BookingStatus, ContactEmail, ContactName, ContactPhone, ExpirationTime, PaymentTransactionId, PaymentMethod, PaymentStatus, CreatedAt, ConfirmedAt) VALUES
(1, 1, 'BK20250630001', 'Confirmed', NULL, NULL, NULL, '2025-07-01 19:30:00', 'TXN20250630001', 'CreditCard', 'Completed', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),
(2, 1, 'BK20250630002', 'Confirmed', NULL, NULL, NULL, '2025-07-01 19:30:00', 'TXN20250630002', 'BankTransfer', 'Completed', '2025-06-30 11:15:00', '2025-06-30 11:20:00'),
(3, 9, 'BK20250630003', 'Confirmed', NULL, NULL, NULL, '2025-07-01 06:00:00', 'TXN20250630003', 'DebitCard', 'Completed', '2025-06-30 14:20:00', '2025-06-30 14:25:00'),
(4, 12, 'BK20250630004', 'Confirmed', NULL, NULL, NULL, '2025-07-01 07:30:00', 'TXN20250630004', 'CreditCard', 'Completed', '2025-06-30 15:45:00', '2025-06-30 15:50:00'),
(5, 1, 'BK20250630005', 'Temporary', NULL, NULL, NULL, NULL, NULL, NULL, 'Pending', '2025-06-30 18:20:00', NULL),
(6, 2, 'BK20250630006', 'Confirmed', NULL, NULL, NULL, '2025-07-01 09:10:00', 'TXN20250630006', 'BankTransfer', 'Completed', '2025-06-30 09:10:00', '2025-06-30 09:15:00'),
(7, 13, 'BK20250630007', 'Confirmed', NULL, NULL, NULL, '2025-07-01 16:30:00', 'TXN20250630007', 'DebitCard', 'Completed', '2025-06-30 16:30:00', '2025-06-30 16:35:00'),
(8, 3, 'BK20250630008', 'Confirmed', NULL, NULL, NULL, '2025-07-01 12:40:00', 'TXN20250630008', 'CreditCard', 'Completed', '2025-06-30 12:40:00', '2025-06-30 12:45:00'),
(9, 10, 'BK20250630009', 'Confirmed', NULL, NULL, NULL, '2025-07-01 13:55:00', 'TXN20250630009', 'Cash', 'Completed', '2025-06-30 13:55:00', '2025-06-30 14:00:00'),
(10, 14, 'BK20250630010', 'Confirmed', NULL, NULL, NULL, '2025-07-01 17:10:00', 'TXN20250630010', 'BankTransfer', 'Completed', '2025-06-30 17:10:00', '2025-06-30 17:15:00'),
(1, 4, 'BK20250630011', 'Cancelled', NULL, NULL, NULL, NULL, NULL, NULL, 'Failed', '2025-06-30 08:20:00', NULL),
(2, 11, 'BK20250630012', 'Confirmed', NULL, NULL, NULL, '2025-07-02 19:05:00', 'TXN20250630012', 'DebitCard', 'Completed', '2025-06-30 19:05:00', '2025-06-30 19:10:00'),
(3, 5, 'BK20250630013', 'Confirmed', NULL, NULL, NULL, '2025-07-01 07:30:00', 'TXN20250630013', 'CreditCard', 'Completed', '2025-06-30 07:30:00', '2025-06-30 07:35:00'),
(4, 6, 'BK20250630014', 'Confirmed', NULL, NULL, NULL, '2025-07-01 20:15:00', 'TXN20250630014', 'BankTransfer', 'Completed', '2025-06-30 20:15:00', '2025-06-30 20:20:00'),
(5, 15, 'BK20250630015', 'Confirmed', NULL, NULL, NULL, '2025-07-01 21:25:00', 'TXN20250630015', 'DebitCard', 'Completed', '2025-06-30 21:25:00', '2025-06-30 21:30:00');

-- Insert sample seat segments
INSERT INTO SeatSegment (TripId, SeatId, SegmentId, BookingId, Status, ReservedAt, BookedAt) VALUES
-- Booking 1: SE1 trip, VIP seat for full journey
(1, 11, 1, 1, 'Booked', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),
(1, 11, 2, 1, 'Booked', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),
(1, 11, 3, 1, 'Booked', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),
(1, 11, 4, 1, 'Booked', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),
(1, 11, 5, 1, 'Booked', '2025-06-30 10:30:00', '2025-06-30 10:35:00'),

-- Booking 2: SE1 trip, Economy seat
(1, 1, 1, 2, 'Booked', '2025-06-30 11:15:00', '2025-06-30 11:20:00'),
(1, 1, 2, 2, 'Booked', '2025-06-30 11:15:00', '2025-06-30 11:20:00'),
(1, 1, 3, 2, 'Booked', '2025-06-30 11:15:00', '2025-06-30 11:20:00'),

-- Booking 3: TN1 trip
(9, 1, 11, 3, 'Booked', '2025-06-30 14:20:00', '2025-06-30 14:25:00'),

-- Temporary booking (Booking 5)
(1, 12, 6, 5, 'TemporaryReserved', '2025-06-30 18:20:00', NULL),
(1, 12, 7, 5, 'TemporaryReserved', '2025-06-30 18:20:00', NULL),

-- More confirmed bookings
(2, 13, 1, 6, 'Booked', '2025-06-30 09:10:00', '2025-06-30 09:15:00'),
(13, 1, 16, 7, 'Booked', '2025-06-30 16:30:00', '2025-06-30 16:35:00'),
(3, 14, 2, 8, 'Booked', '2025-06-30 12:40:00', '2025-06-30 12:45:00'),
(10, 2, 11, 9, 'Booked', '2025-06-30 13:55:00', '2025-06-30 14:00:00'),
(14, 15, 17, 10, 'Booked', '2025-06-30 17:10:00', '2025-06-30 17:15:00'),
(11, 3, 11, 12, 'Booked', '2025-06-30 19:05:00', '2025-06-30 19:10:00'),
(5, 4, 12, 13, 'Booked', '2025-06-30 07:30:00', '2025-06-30 07:35:00'),
(6, 5, 13, 14, 'Booked', '2025-06-30 20:15:00', '2025-06-30 20:20:00'),
(15, 6, 18, 15, 'Booked', '2025-06-30 21:25:00', '2025-06-30 21:30:00');

-- =====================================================
-- PHẦN TIẾP THEO - HOÀN THIỆN DỮ LIỆU MẪU
-- =====================================================

-- Insert sample tickets (tiếp tục từ chỗ bị cắt)
INSERT INTO Ticket (BookingId, UserId, TripId, TicketCode, PassengerName, PassengerIdCard, PassengerPhone, TotalPrice, FinalPrice, Status) VALUES
(1, 1, 1, 'TK20250630001', N'Nguyễn Văn A', '123456789012', '0901234567', 1250000.00, 1250000.00, 'Valid'),
(2, 2, 1, 'TK20250630002', N'Trần Thị B', '234567890123', '0912345678', 890000.00, 890000.00, 'Valid'),
(3, 3, 9, 'TK20250630003', N'Lê Quang C', '345678901234', '0923456789', 320000.00, 320000.00, 'Valid'),
(4, 4, 12, 'TK20250630004', N'Phạm Thị D', '456789012345', '0934567890', 450000.00, 450000.00, 'Valid'),
(6, 6, 2, 'TK20250630006', N'Vũ Thị F', '567890123456', '0956789012', 1350000.00, 1350000.00, 'Valid'),
(7, 7, 13, 'TK20250630007', N'Đặng Văn G', '678901234567', '0967890123', 680000.00, 680000.00, 'Valid'),
(8, 8, 3, 'TK20250630008', N'Bùi Trí H', '789012345678', '0978901234', 1450000.00, 1450000.00, 'Valid'),
(9, 9, 10, 'TK20250630009', N'Đỗ Thị I', '890123456789', '0989012345', 285000.00, 285000.00, 'Valid'),
(10, 10, 14, 'TK20250630010', N'Mai Quỳnh J', '901234567890', '0990123456', 720000.00, 720000.00, 'Valid'),
(12, 2, 11, 'TK20250630012', N'Trần Thị B', '234567890123', '0912345678', 310000.00, 310000.00, 'Valid'),
(13, 3, 5, 'TK20250630013', N'Lê Quang C', '345678901234', '0923456789', 1380000.00, 1380000.00, 'Valid'),
(14, 4, 6, 'TK20250630014', N'Phạm Thị D', '456789012345', '0934567890', 1420000.00, 1420000.00, 'Valid'),
(15, 5, 15, 'TK20250630015', N'Hoàng Văn E', '567890123457', '0945678901', 590000.00, 590000.00, 'Valid');

-- Insert ticket segments
INSERT INTO TicketSegment (TicketId, SegmentId, SeatId, SegmentPrice, DepartureTime, ArrivalTime, CheckInStatus) VALUES
-- Ticket 1: Full journey segments
(1, 1, 11, 125000.00, '2025-07-01 19:30:00', '2025-07-02 01:30:00', 'NotCheckedIn'),
(1, 2, 11, 125000.00, '2025-07-02 02:00:00', '2025-07-02 08:00:00', 'NotCheckedIn'),
(1, 3, 11, 125000.00, '2025-07-02 08:30:00', '2025-07-02 10:30:00', 'NotCheckedIn'),
(1, 4, 11, 125000.00, '2025-07-02 11:00:00', '2025-07-02 13:30:00', 'NotCheckedIn'),
(1, 5, 11, 125000.00, '2025-07-02 14:00:00', '2025-07-02 18:00:00', 'NotCheckedIn'),

-- Ticket 2: Partial journey segments  
(2, 1, 1, 89000.00, '2025-07-01 19:30:00', '2025-07-02 01:30:00', 'NotCheckedIn'),
(2, 2, 1, 89000.00, '2025-07-02 02:00:00', '2025-07-02 08:00:00', 'NotCheckedIn'),
(2, 3, 1, 89000.00, '2025-07-02 08:30:00', '2025-07-02 10:30:00', 'NotCheckedIn'),

-- Ticket 3: Short trip
(3, 11, 1, 320000.00, '2025-07-01 06:00:00', '2025-07-01 12:00:00', 'NotCheckedIn'),

-- Other tickets
(4, 16, 1, 150000.00, '2025-07-01 07:30:00', '2025-07-01 09:00:00', 'NotCheckedIn'),
(4, 17, 1, 150000.00, '2025-07-01 09:30:00', '2025-07-01 13:30:00', 'NotCheckedIn'),
(4, 18, 1, 150000.00, '2025-07-01 14:00:00', '2025-07-01 16:30:00', 'NotCheckedIn'),

(5, 1, 13, 135000.00, '2025-07-02 19:00:00', '2025-07-03 01:00:00', 'NotCheckedIn'),
(6, 16, 1, 680000.00, '2025-07-01 22:30:00', '2025-07-02 06:30:00', 'NotCheckedIn'),
(7, 2, 14, 145000.00, '2025-07-01 22:00:00', '2025-07-02 04:00:00', 'NotCheckedIn'),
(8, 11, 2, 285000.00, '2025-07-01 14:00:00', '2025-07-01 20:00:00', 'NotCheckedIn'),
(9, 17, 15, 720000.00, '2025-07-02 22:30:00', '2025-07-03 06:30:00', 'NotCheckedIn'),
(10, 11, 3, 310000.00, '2025-07-02 06:00:00', '2025-07-02 12:00:00', 'NotCheckedIn'),
(11, 12, 4, 138000.00, '2025-07-03 19:30:00', '2025-07-04 01:30:00', 'NotCheckedIn'),
(12, 13, 5, 142000.00, '2025-07-04 19:00:00', '2025-07-05 01:00:00', 'NotCheckedIn'),
(13, 18, 6, 590000.00, '2025-07-02 22:30:00', '2025-07-03 06:30:00', 'NotCheckedIn');

-- Insert fare data
INSERT INTO Fare (RouteId, SegmentId, SeatClass, SeatType, BasePrice, EffectiveFrom, EffectiveTo) VALUES
-- Route 1 (Hà Nội - TP.HCM) fares
(1, 1, 'Economy', 'Window', 89000.00, '2025-01-01 00:00:00', NULL),
(1, 1, 'Economy', 'Aisle', 85000.00, '2025-01-01 00:00:00', NULL),
(1, 1, 'Business', 'Window', 145000.00, '2025-01-01 00:00:00', NULL),
(1, 1, 'Business', 'Aisle', 140000.00, '2025-01-01 00:00:00', NULL),
(1, 1, 'VIP', 'Window', 225000.00, '2025-01-01 00:00:00', NULL),
(1, 2, 'Economy', 'Window', 95000.00, '2025-01-01 00:00:00', NULL),
(1, 2, 'Economy', 'Aisle', 92000.00, '2025-01-01 00:00:00', NULL),
(1, 2, 'Business', 'Window', 155000.00, '2025-01-01 00:00:00', NULL),
(1, 2, 'VIP', 'Window', 245000.00, '2025-01-01 00:00:00', NULL),
(1, 3, 'Economy', 'Window', 45000.00, '2025-01-01 00:00:00', NULL),
(1, 3, 'Business', 'Window', 75000.00, '2025-01-01 00:00:00', NULL),
(1, 3, 'VIP', 'Window', 125000.00, '2025-01-01 00:00:00', NULL),

-- Route 2 (Hà Nội - Vinh) fares
(2, 11, 'Economy', 'Window', 320000.00, '2025-01-01 00:00:00', NULL),
(2, 11, 'Economy', 'Aisle', 310000.00, '2025-01-01 00:00:00', NULL),
(2, 11, 'Business', 'Window', 480000.00, '2025-01-01 00:00:00', NULL);

-- Insert payment records
INSERT INTO Payment (BookingId, PaymentMethod, Amount, TransactionId, GatewayTransactionId, PaymentGateway, Status, PaymentTime, ConfirmedTime) VALUES
(1, 'CreditCard', 1250000.00, 'TXN20250630001', 'VISA_123456789', 'VNPay', 'Completed', '2025-06-30 10:32:00', '2025-06-30 10:35:00'),
(2, 'BankTransfer', 890000.00, 'TXN20250630002', 'BANK_987654321', 'VietComBank', 'Completed', '2025-06-30 11:17:00', '2025-06-30 11:20:00'),
(3, 'DebitCard', 320000.00, 'TXN20250630003', 'DEBIT_456789123', 'VNPay', 'Completed', '2025-06-30 14:22:00', '2025-06-30 14:25:00'),
(4, 'CreditCard', 450000.00, 'TXN20250630004', 'MASTER_789123456', 'MoMo', 'Completed', '2025-06-30 15:47:00', '2025-06-30 15:50:00'),
(6, 'BankTransfer', 1350000.00, 'TXN20250630006', 'BANK_147258369', 'Techcombank', 'Completed', '2025-06-30 09:12:00', '2025-06-30 09:15:00'),
(7, 'DebitCard', 680000.00, 'TXN20250630007', 'DEBIT_369258147', 'VNPay', 'Completed', '2025-06-30 16:32:00', '2025-06-30 16:35:00'),
(8, 'CreditCard', 1450000.00, 'TXN20250630008', 'VISA_852741963', 'ZaloPay', 'Completed', '2025-06-30 12:42:00', '2025-06-30 12:45:00'),
(9, 'Cash', 285000.00, 'TXN20250630009', 'CASH_741852963', 'Counter', 'Completed', '2025-06-30 13:57:00', '2025-06-30 14:00:00'),
(10, 'BankTransfer', 720000.00, 'TXN20250630010', 'BANK_963852741', 'ACB', 'Completed', '2025-06-30 17:12:00', '2025-06-30 17:15:00'),
(12, 'DebitCard', 310000.00, 'TXN20250630012', 'DEBIT_159753486', 'VNPay', 'Completed', '2025-06-30 19:07:00', '2025-06-30 19:10:00'),
(13, 'CreditCard', 1380000.00, 'TXN20250630013', 'MASTER_486159753', 'ShopeePay', 'Completed', '2025-06-30 07:32:00', '2025-06-30 07:35:00'),
(14, 'BankTransfer', 1420000.00, 'TXN20250630014', 'BANK_753486159', 'VietinBank', 'Completed', '2025-06-30 20:17:00', '2025-06-30 20:20:00'),
(15, 'DebitCard', 590000.00, 'TXN20250630015', 'DEBIT_357159486', 'MoMo', 'Completed', '2025-06-30 21:27:00', '2025-06-30 21:30:00'),
(11, 'CreditCard', 1200000.00, 'TXN20250630011', 'VISA_999888777', 'VNPay', 'Failed', '2025-06-30 08:22:00', NULL),
(5, 'CreditCard', 1100000.00, 'TXN20250630005', 'PENDING_123456', 'VNPay', 'Pending', '2025-06-30 18:22:00', NULL);

-- Insert notifications
INSERT INTO Notification (UserId, Type, Title, Message, Priority, RelatedEntityType, RelatedEntityId, EmailSent, SmsSent) VALUES
(1, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu SE1 từ Hà Nội đi TP.HCM ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630001', 'High', 'Booking', 1, 1, 1),
(2, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu SE1 từ Hà Nội đi TP.HCM ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630002', 'High', 'Booking', 2, 1, 0),
(3, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu TN1 từ Hà Nội đi Vinh ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630003', 'High', 'Booking', 3, 1, 1),
(4, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu SPT1 từ TP.HCM đi Nha Trang ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630004', 'High', 'Booking', 4, 1, 0),
(5, 'BookingExpiring', N'Cảnh báo đặt chỗ sắp hết hạn', N'Đặt chỗ của bạn sẽ hết hạn trong 2 phút. Vui lòng hoàn tất thanh toán.', 'Critical', 'Booking', 5, 1, 1),
(6, 'PaymentSuccess', N'Thanh toán thành công', N'Thanh toán cho booking BK20250630006 đã được xử lý thành công.', 'High', 'Payment', 6, 1, 0),
(7, 'TripDelay', N'Thông báo chuyến tàu bị delay', N'Chuyến tàu SNT1 ngày 01/07/2025 có thể bị delay 15 phút do thời tiết.', 'Normal', 'Trip', 13, 0, 1),
(8, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu SE3 từ Hà Nội đi TP.HCM ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630008', 'High', 'Booking', 8, 1, 1),
(9, 'BookingConfirmation', N'Xác nhận đặt vé thành công', N'Vé tàu TN1 từ Hà Nội đi Vinh ngày 01/07/2025 đã được đặt thành công. Mã vé: TK20250630009', 'High', 'Booking', 9, 1, 0),
(10, 'SpecialOffer', N'Ưu đãi đặc biệt cho khách hàng thân thiết', N'Giảm 10% cho chuyến đi tiếp theo. Mã giảm giá: LOYAL10', 'Normal', NULL, NULL, 1, 0),
(1, 'PaymentFailed', N'Thanh toán thất bại', N'Thanh toán cho booking BK20250630011 đã thất bại. Vui lòng thử lại.', 'High', 'Payment', 14, 1, 1),
(2, 'TripReminder', N'Nhắc nhở chuyến đi', N'Chuyến tàu SE2 của bạn khởi hành sau 24 giờ nữa. Vui lòng có mặt tại ga trước 30 phút.', 'Normal', 'Trip', 2, 1, 1),
(3, 'MaintenanceNotice', N'Thông báo bảo trì hệ thống', N'Hệ thống sẽ bảo trì từ 2:00-4:00 sáng ngày 02/07/2025.', 'Low', NULL, NULL, 0, 0),
(11, 'SystemAlert', N'Cảnh báo hệ thống', N'Phát hiện hoạt động bất thường trong hệ thống thanh toán.', 'Critical', NULL, NULL, 1, 0),
(12, 'NewFeature', N'Tính năng mới', N'Giờ đây bạn có thể chọn chỗ ngồi trực tuyến khi đặt vé!', 'Normal', NULL, NULL, 1, 0);

-- Insert price calculation logs
INSERT INTO PriceCalculationLog (BookingId, TripId, SegmentId, SeatClass, SeatType, BasePrice, FinalPrice, PricingMethod, PricingFactors, UserId) VALUES
(1, 1, 1, 'VIP', 'Window', 225000.00, 250000.00, 'AI', '{"occupancy_rate": 0.65, "days_before_departure": 2, "season": "summer", "weekend": false}', 1),
(1, 1, 2, 'VIP', 'Window', 245000.00, 270000.00, 'AI', '{"occupancy_rate": 0.70, "days_before_departure": 2, "season": "summer", "weekend": false}', 1),
(2, 1, 1, 'Economy', 'Window', 89000.00, 89000.00, 'Traditional', '{"base_fare": true, "no_adjustment": true}', 2),
(2, 1, 2, 'Economy', 'Window', 95000.00, 95000.00, 'Traditional', '{"base_fare": true, "no_adjustment": true}', 2),
(3, 9, 11, 'Economy', 'Window', 320000.00, 320000.00, 'Traditional', '{"base_fare": true, "short_distance": true}', 3),
(4, 12, 16, 'Economy', 'Window', 150000.00, 150000.00, 'AI', '{"occupancy_rate": 0.45, "days_before_departure": 1, "route_popularity": "high"}', 4),
(6, 2, 1, 'Economy', 'Window', 89000.00, 135000.00, 'AI', '{"occupancy_rate": 0.80, "days_before_departure": 2, "demand_surge": 1.5}', 6),
(7, 13, 16, 'Business', 'Window', 480000.00, 680000.00, 'AI', '{"occupancy_rate": 0.90, "days_before_departure": 1, "premium_demand": true}', 7),
(8, 3, 2, 'Business', 'Window', 155000.00, 145000.00, 'Traditional', '{"base_fare": true, "early_booking_discount": 0.05}', 8),
(9, 10, 11, 'Economy', 'Aisle', 310000.00, 285000.00, 'AI', '{"occupancy_rate": 0.30, "days_before_departure": 1, "low_demand": true}', 9),
(10, 14, 17, 'Business', 'Window', 600000.00, 720000.00, 'AI', '{"occupancy_rate": 0.85, "days_before_departure": 2, "night_train_premium": true}', 10),
(12, 11, 11, 'Economy', 'Window', 320000.00, 310000.00, 'Traditional', '{"base_fare": true, "return_customer_discount": 0.03}', 2),
(13, 5, 12, 'Economy', 'Window', 142000.00, 138000.00, 'AI', '{"occupancy_rate": 0.40, "days_before_departure": 3, "off_peak": true}', 3),
(14, 6, 13, 'Economy', 'Window', 145000.00, 142000.00, 'Traditional', '{"base_fare": true, "loyalty_discount": 0.02}', 4),
(15, 15, 18, 'Business', 'Window', 580000.00, 590000.00, 'AI', '{"occupancy_rate": 0.60, "days_before_departure": 2, "route_premium": true}', 5);

-- =====================================================
-- VIEWS VÀ PROCEDURES HỮU ÍCH
-- =====================================================


