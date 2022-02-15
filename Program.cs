
// for cors
using autobus_complete.Hubs;

var _loginOrigin = "_localorigin";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(_loginOrigin, builder =>
    {
        builder.WithOrigins("https://autobus-complete.netlify.app");
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.AllowCredentials();

    });
});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors(_loginOrigin);
app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/GameHub");

app.Run();
