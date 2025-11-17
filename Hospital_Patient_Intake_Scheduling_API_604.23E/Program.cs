using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Services;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication - FIXED VERSION
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

        // Detailed debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("=== JWT AUTHENTICATION FAILED ===");
                Console.WriteLine($"Exception: {context.Exception}");
                Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                if (context.Exception is SecurityTokenExpiredException)
                    Console.WriteLine("Token is expired");
                else if (context.Exception is SecurityTokenInvalidIssuerException)
                    Console.WriteLine("Token issuer is invalid");
                else if (context.Exception is SecurityTokenInvalidAudienceException)
                    Console.WriteLine("Token audience is invalid");
                else if (context.Exception is SecurityTokenInvalidSignatureException)
                    Console.WriteLine("Token signature is invalid");
                Console.WriteLine("=== END AUTH FAILED ===");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("=== JWT TOKEN VALIDATED SUCCESSFULLY ===");
                Console.WriteLine($"User: {context.Principal.Identity.Name}");
                Console.WriteLine($"Claims: {string.Join(", ", context.Principal.Claims.Select(c => c.Type))}");
                Console.WriteLine("=== END TOKEN VALIDATED ===");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"=== TOKEN RECEIVED ===");
                Console.WriteLine($"Token: {context.Token}");
                Console.WriteLine($"=== END TOKEN RECEIVED ===");
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Debug: Check if users exist
    var users = context.Users.ToList();
    Console.WriteLine($"=== DATABASE USERS COUNT: {users.Count} ===");
    foreach (var user in users)
    {
        Console.WriteLine($"User: {user.Username}, Role: {user.Role}");
    }
}

app.Run();