using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// 👇 Đăng ký MVC
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TrainBookingSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 👇 Đăng ký HttpClient cho TicketService với link đến backend API
builder.Services.AddHttpClient<ISearchService, SearchService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5014/api/"); 
});
builder.Services.AddHttpClient<ITripService, TripService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5014/");
});
builder.Services.AddHttpClient<ISeatService, SeatService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5014/api"); // Địa chỉ của Web API
});
builder.Services.AddScoped<IBasicDataService, BasicDataService>();
var app = builder.Build();

// ✅ Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();         
app.UseRouting();
app.UseAuthorization();        
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
