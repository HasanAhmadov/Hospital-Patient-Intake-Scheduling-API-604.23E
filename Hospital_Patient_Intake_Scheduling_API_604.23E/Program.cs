using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Services;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Utility;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hospital Management API",
        Version = "v1",
        Description = "API for hospital patient intake and scheduling system"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure Entity Framework with PostgreSQL
var connectionString = ConnectionHelper.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Debug events to help with JWT issues
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"JWT Token Validated for user: {context.Principal.Identity.Name}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ✅ FIXED Swagger Protection Middleware
app.Use(async (context, next) =>
{
    // Only protect Swagger UI routes
    if (context.Request.Path.StartsWithSegments("/swagger") ||
        context.Request.Path.StartsWithSegments("/swagger-ui") ||
        context.Request.Path == "/" || // Redirect root to Swagger
        context.Request.Path == "/index.html")
    {
        // Skip auth for Swagger JSON files and CSS/JS assets
        if (context.Request.Path.StartsWithSegments("/swagger/v1/swagger.json") ||
            context.Request.Path.StartsWithSegments("/swagger-ui/") ||
            context.Request.Path.Value.EndsWith(".css") ||
            context.Request.Path.Value.EndsWith(".js") ||
            context.Request.Path.Value.EndsWith(".png"))
        {
            await next();
            return;
        }

        string authHeader = context.Request.Headers["Authorization"];

        // Check if Basic Auth header is present and valid
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
        {
            try
            {
                var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var parts = decodedUsernamePassword.Split(':', 2);

                // Get credentials from environment variables or use defaults
                var expectedUsername = Environment.GetEnvironmentVariable("SWAGGER_USERNAME") ?? "admin";
                var expectedPassword = Environment.GetEnvironmentVariable("SWAGGER_PASSWORD") ?? "2025Secure_API!#Sw@g3r";

                if (parts.Length == 2 && parts[0] == expectedUsername && parts[1] == expectedPassword)
                {
                    await next();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Basic auth parsing error: {ex.Message}");
            }
        }

        // Return 401 with Basic Auth challenge
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hospital API Swagger UI\", charset=\"UTF-8\"";
        context.Response.StatusCode = 401;

        // For HTML requests, return a simple unauthorized message
        if (context.Request.Headers["Accept"].ToString().Contains("text/html"))
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(@"
                <html>
                    <head><title>401 Unauthorized</title></head>
                    <body>
                        <h1>401 Unauthorized</h1>
                        <p>Access to Swagger documentation requires authentication.</p>
                        <p>Use credentials: admin / 2025Secure_API!#Sw@g3r</p>
                    </body>
                </html>");
        }
        else
        {
            await context.Response.WriteAsync("Unauthorized");
        }
        return;
    }

    // ✅ ALL other routes (your API endpoints) pass through normally
    await next();
});

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    c.DisplayRequestDuration();
});

// Redirect root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Apply migrations
        await context.Database.MigrateAsync();
        Console.WriteLine("Database migrated successfully.");

        // Seed data
        await DataHelper.ManageDataAsync(scope.ServiceProvider);
        Console.WriteLine("Data seeding completed.");

        // Debug: Check if users exist
        var users = context.Users.ToList();
        Console.WriteLine($"=== DATABASE USERS COUNT: {users.Count} ===");
        foreach (var user in users)
        {
            Console.WriteLine($"User: {user.Username}, Role: {user.Role}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database operation failed: {ex.Message}");
        // Don't throw - let the app start even if migration fails
    }
}

app.Run();