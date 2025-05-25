using FileStoringService.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(
    "http://localhost:5002",   
    "https://localhost:5003"   
);

builder.Services.AddSingleton<Bd>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();