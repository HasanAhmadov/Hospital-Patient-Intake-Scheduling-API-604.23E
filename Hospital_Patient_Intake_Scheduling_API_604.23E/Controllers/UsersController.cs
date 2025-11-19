using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can access all endpoints in this controller
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return BadRequest("Username already exists");
            }

            // Validate role
            if (!IsValidRole(createUserDto.Role))
            {
                return BadRequest("Invalid role. Must be: Receptionist, Doctor, or Admin");
            }

            var user = new User
            {
                Username = createUserDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Role = createUserDto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if username already exists (if changing username)
            if (!string.IsNullOrEmpty(updateUserDto.Username) &&
                updateUserDto.Username != user.Username &&
                await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username))
            {
                return BadRequest("Username already exists");
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateUserDto.Username))
            {
                user.Username = updateUserDto.Username;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            if (!string.IsNullOrEmpty(updateUserDto.Role))
            {
                if (!IsValidRole(updateUserDto.Role))
                {
                    return BadRequest("Invalid role. Must be: Receptionist, Doctor, or Admin");
                }
                user.Role = updateUserDto.Role;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the last admin
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    return BadRequest("Cannot delete the last admin user");
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to validate roles
        private static bool IsValidRole(string role)
        {
            return role == "Receptionist" || role == "Doctor" || role == "Admin";
        }
    }
}