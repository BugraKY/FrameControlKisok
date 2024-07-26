using FrameControlKiosk.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FrameControlKiosk.Extensions;
using System.Net;
using Microsoft.AspNetCore.WebSockets;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionDatabase")));

//var allowedIps= ApplicationDbContext


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAnyOrigins",
        builder =>
        {
            builder.WithOrigins("*");
            builder.AllowAnyMethod();
            builder.AllowAnyHeader();
        });
});

// Kestrel sunucusu için çoklu IP adresi ve port yapýlandýrmasý
/*
builder.WebHost.ConfigureKestrel(serverOptions =>
{

    serverOptions.Listen(IPAddress.Parse("192.168.1.2"), 5000);


    serverOptions.Listen(IPAddress.Parse("192.168.1.3"), 5001);

    serverOptions.ListenAnyIP(5002);
});
*/
builder.Services.AddCors();
builder.Services.AddControllers();

builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
builder.Services.AddSignalR();
builder.Services.AddLogging(
    builder =>
    {
        builder.AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("NToastNotify", LogLevel.Warning)
        .AddConsole();
    });



var allowedIp = builder.Configuration["AllowedIp"];

var app = builder.Build();
app.UseMiddleware<IpRestrictionMiddleware>(allowedIp);
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //var ipAddresses = dbContext.Station.Select(x => x.IP).ToList();



    Services.DetecthDevices(dbContext);
    /*
    app.UseCors(
        policy => policy
        //.WithOrigins(allowedIps.ToArray())
        .WithOrigins("192.168.0.68")
        .AllowAnyHeader()
        .AllowAnyMethod()
    );*/

}
app.UseCors("AllowAnyOrigins");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

//app.UseCors("AllowAnyOrigins");

app.UseRouting();

app.UseAuthorization();

app.MapHub<SignalHub>("/signalhub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
