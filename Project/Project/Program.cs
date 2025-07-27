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
//using Project.Services.Seat;
using Project.Services.Train;
using Project.Utils.Validation;
using Project.Swagger;

using Project.Repository;

using Project.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Project.Services.Metrics;



var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add DbContext
builder.Services.AddDbContext<FastRailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add simple services
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQRService, QRService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();

builder.Services.AddAutoMapper(typeof(TrainProfile));
builder.Services.AddAutoMapper(typeof(RouteProfile));
builder.Services.AddAutoMapper(typeof(CarriageProfile));
builder.Services.AddAutoMapper(typeof(TripProfile));
builder.Services.AddScoped<IFareService, FareService>();
builder.Services.AddScoped<ITicketService, TicketService>();

//Config DI
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IFareRepository, FareRepository>();
builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ITrainService, TrainServices>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ICarriageRepository, CarriageRepository>();
builder.Services.AddScoped<ICarriageService, CarriageService>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ItripRepository, TripRepository>();
builder.Services.AddScoped<ITripService, TripService>();

//Add Fluent Validation 
builder.Services.AddScoped<IValidator<TripDTO>, TripValidator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FastRail API", Version = "v1" });
    
    // Configure file upload support
    c.OperationFilter<FileUploadOperationFilter>();
    
    // Add JWT authentication support
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddLogging();
var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();



app.Run();

