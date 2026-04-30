using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
//using Write.Write_to_DB3.WriteDB;  // пространство имён, где лежит WriteDB и User
using Write;  // пространство имён, где лежит WriteDB и User

using Web_Registration.Services;

var builder = WebApplication.CreateBuilder(args);

// API (контроллеры)
builder.Services.AddControllers();

// Веб-UI (Razor Pages) — только для /register
builder.Services.AddRazorPages();

// Swagger удобно для проверки API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Сервисы, куда мы переносим логику
builder.Services.AddScoped<BirthDateValidator, BirthDateValidator>();
builder.Services.AddScoped<WriteDbService, WriteDbService>();

// Если у тебя есть стартовая инициализация WriteDB (Start) — включим:
builder.Services.AddHostedService<CheckingDbThread>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//сервер делает редирект на HTTPS
app.UseHttpsRedirection();

// маршруты UI и API, важные команды чтобы не было ошибки 
app.MapRazorPages();
app.MapControllers();

app.Run();
