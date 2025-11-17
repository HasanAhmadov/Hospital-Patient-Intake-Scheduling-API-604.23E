using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            Console.WriteLine($"=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Username: {loginDto.Username}");

            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                Console.WriteLine($"❌ LOGIN FAILED: User '{loginDto.Username}' not found");
                return null;
            }

            Console.WriteLine($"✅ User found: {user.Username}");

            // Verify password with BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                Console.WriteLine($"❌ LOGIN FAILED: Invalid password for user '{loginDto.Username}'");
                return null;
            }

            Console.WriteLine($"✅ LOGIN SUCCESS: User '{user.Username}' authenticated");

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Log token details for debugging
            Console.WriteLine($"🔑 TOKEN GENERATED for {user.Username}");
            Console.WriteLine($"   Role: {user.Role}");
            Console.WriteLine($"   Issuer: {_configuration["Jwt:Issuer"]}");
            Console.WriteLine($"   Audience: {_configuration["Jwt:Audience"]}");
            Console.WriteLine($"=== END LOGIN ===");

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                Expires = DateTime.UtcNow.AddHours(3)
            };
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            Console.WriteLine($"=== GENERATING TOKEN ===");
            Console.WriteLine($"   JWT Key length: {jwtKey?.Length}");
            Console.WriteLine($"   Issuer: {jwtIssuer}");
            Console.WriteLine($"   Audience: {jwtAudience}");
            Console.WriteLine($"   User: {user.Username}");
            Console.WriteLine($"   Role: {user.Role}");

            // CRITICAL: Create claims that match what your app expects
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("role", user.Role)
            };

            Console.WriteLine($"   Claims added: {claims.Count}");

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            Console.WriteLine($"✅ TOKEN DETAILS:");
            Console.WriteLine($"   Length: {tokenString.Length} characters");
            Console.WriteLine($"   Expires: {token.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"   First 50 chars: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");
            Console.WriteLine($"=== END TOKEN GENERATION ===");

            return tokenString;
        }
    }
}