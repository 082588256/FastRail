using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Project;
using Project.DTOs;
using Project.MappingProfile;
using Project.Repository.Carriage;
using Project.Repository.Route;
using Project.Repository.Seat;
using Project.Repository.Train;
using Project.Repository.Trip;
using Project.Services;
using Project.Services.Carriage;
using Project.Services.Route;
using Project.Services.Seat;
using Project.Services.Train;
using Project.Utils.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<FastRailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add simple services
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAutoMapper(typeof(TrainProfile));
builder.Services.AddAutoMapper(typeof(RouteProfile));
builder.Services.AddAutoMapper(typeof(CarriageProfile));
builder.Services.AddAutoMapper(typeof(TripProfile));

//Config DI
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteService, RouteService>();

builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ITrainService, TrainServices>();

builder.Services.AddScoped<ICarriageRepository, CarriageRepository>();
builder.Services.AddScoped<ICarriageService, CarriageService>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ItripRepository, TripRepository>();
builder.Services.AddScoped<ITripService, TripService>();

//Add Fluent Validation 
builder.Services.AddScoped<IValidator<TripDTO>, TripValidator>();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();
var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

