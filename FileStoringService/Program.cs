using FileStoringService.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(
    "http://localhost:5002",
    "https://localhost:5003"
);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FileStoreContext>(opts =>
    opts.UseNpgsql(conn));

builder.Services.AddSingleton<Bd>();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileStoreContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();