using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.Options;
using ProjectAssetAPI.Security;

var builder = WebApplication.CreateBuilder(args);

// Configuración y opciones
builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection(SecurityOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<VisualDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProjectAssetAPI",
        Version = "v1",
        Description = "API para inventario de dispositivos (VisualLabs) y eventos de seguridad (VisualSecurity)."
    });

    options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = ApiKeyAuthenticationHandler.HeaderName,
        In = ParameterLocation.Header,
        Description = $"Clave de acceso enviada en el encabezado {ApiKeyAuthenticationHandler.HeaderName}."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationHandler.SchemeName, document)] = []
    });
});

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000", "http://127.0.0.1:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestampUtc = DateTime.UtcNow
})).AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program { }
