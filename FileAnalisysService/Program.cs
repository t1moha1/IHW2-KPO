using FileAnalisysService.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Db>();
builder.Services.AddControllers();

builder.WebHost.UseUrls(
    "http://localhost:5004",   
    "https://localhost:5005"   
);

builder.Services.AddHttpClient("FileService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/api/");
});

var app = builder.Build();

app.MapControllers();

app.Run();