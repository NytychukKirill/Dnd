using System.Text.Json.Serialization;
using DnD.Data;
using DnD.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. База данных (оставьте вашу строку подключения из текущего Program.cs)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Контроллеры + конвертация enum Ability (Strength, Dexterity...) из строк JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 3. SignalR для обновлений комнаты в реальном времени
builder.Services.AddSignalR();

// 4. CORS для Vite фронтенда (http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Обязательно для SignalR WebSockets
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

app.MapControllers();
app.MapHub<RoomHub>("/roomHub");

app.Run();