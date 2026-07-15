using System.Text;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudySpot.Components;
using StudySpot.Data;
using StudySpot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Load .env file
var envFile = Path.Combine(
    builder.Environment.ContentRootPath,
    ".env");

if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var parts = line.Split('=', 2);

        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(
                parts[0].Trim(),
                parts[1].Trim());
        }
    }
}

// Blazor
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// Configuration
builder.Configuration.AddUserSecrets<Program>();

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("JWT Key is missing from configuration.");

// Database
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString =
        $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
        $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
        $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
        $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
        $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
        "SSL Mode=Require;" +
        "Trust Server Certificate=true";
}

builder.Services.AddDbContext<StudySpotContext>(options =>
    options.UseNpgsql(connectionString));

// Application services
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AmenityService>();
builder.Services.AddScoped<RoomAmenityService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthService>();


// Local storage
builder.Services.AddBlazoredLocalStorage();

// Authentication
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();