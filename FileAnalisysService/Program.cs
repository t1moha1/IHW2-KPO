using FileAnalisysService.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AnalysisContext>(opts =>
    opts.UseNpgsql(conn));

builder.WebHost.UseUrls(
    "http://localhost:5004",   
    "https://localhost:5005"   
);

builder.Services.AddHttpClient("FileService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/api/");
});

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalysisContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();