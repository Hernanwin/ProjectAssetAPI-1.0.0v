using Microsoft.EntityFrameworkCore;
using ProjectAssetAPI.Context;
//using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connetionString = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<VisualDBContext>(options => options.UseSqlServer(connetionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
options.AddPolicy("NextJs", policy =>
{
policy.WithOrigins("http://localhost:3000") // Puerto Next.js
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("NextJs");

app.Run();
