using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.MappingProfile;
using Project.Models;
using Project.Repository.Carriage;
using Project.Repository.Route;
using Project.Repository.Seat;
using Project.Repository.Train;
using Project.Repository.Trip;
using Project.Services.Carriage;
using Project.Services.Route;
using Project.Services.Seat;
using Project.Services.Train;
using Project.Services.Trip;
using Project.Utils.Validation;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Add DbContext and configure SQL Server connection
            builder.Services.AddDbContext<FastRailDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //AutoMapper Config
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
            var app = builder.Build();

            // Kiểm tra kết nối database và log kết quả
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var dbContext = services.GetRequiredService<FastRailDbContext>();
                    dbContext.Database.OpenConnection(); // Thử mở kết nối
                    logger.LogInformation("Kết nối database thành công!");
                    dbContext.Database.CloseConnection();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kết nối database thất bại!");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}