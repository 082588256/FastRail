using Microsoft.EntityFrameworkCore;
using Project.MappingProfile;
using Project.Models;
using Project.Repository.Route;
using Project.Repository.Train;
using Project.Services.Route;
using Project.Services.Train;
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

            //Config DI
            builder.Services.AddScoped<IRouteRepository, RouteRepository>();
            builder.Services.AddScoped<IRouteService, RouteService>();

            builder.Services.AddScoped<ITrainRepository, TrainRepository>();
            builder.Services.AddScoped<ITrainService, TrainServices>();
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