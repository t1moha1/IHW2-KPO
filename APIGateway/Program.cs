using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title   = "Gateway API",
        Version = "v1"
    });
});



builder.WebHost.UseUrls(
    "http://0.0.0.0:6000",
    "https://0.0.0.0:6001"
);

builder.Services.AddHttpClient("FileService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/api/");
});

builder.Services.AddHttpClient("AnalisysService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5004/api/");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();