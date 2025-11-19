using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Allow Admins and Receptionists to view/manage doctors
    [Authorize(Roles = "Admin")]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/doctors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
        {
            var doctors = await _context.Set<Doctor>() // Use Set<Doctor>() for clarity with TPH
                .Where(d => d.IsActive)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    UserName = d.Username,
                    Name = d.Name,
                    Specialty = d.Specialty,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // GET: /api/doctors/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _context.Set<Doctor>()
                .Where(d => d.Id == id && d.IsActive)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    UserName = d.Username,
                    Name = d.Name,
                    Specialty = d.Specialty,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive
                })
                .FirstOrDefaultAsync();

            if (doctor == null) return NotFound();

            return doctor;
        }

        // POST: /api/doctors
        // Creation of a new Doctor (which is also a User) is restricted to Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDto>> CreateDoctor(RegisterDoctorDto registerDoctorDto)
        {
            // Input Validation: Check for existing username/email
            if (await _context.Users.AnyAsync(u => u.Username == registerDoctorDto.Username))
            {
                return BadRequest("Username already exists.");
            }
            if (await _context.Set<Doctor>().AnyAsync(d => d.Email == registerDoctorDto.Email))
            {
                return BadRequest("Email already used by a doctor.");
            }

            // Create Doctor, initializing both base (User) and derived properties
            var doctor = new Doctor
            {
                Username = registerDoctorDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDoctorDto.Password), // Hashing password
                Role = "Doctor", // Assigning the specific Role

                Name = registerDoctorDto.Name,
                Specialty = registerDoctorDto.Specialty,
                PhoneNumber = registerDoctorDto.PhoneNumber,
                Email = registerDoctorDto.Email,
                IsActive = true
            };

            _context.Set<Doctor>().Add(doctor);
            await _context.SaveChangesAsync();

            var doctorDto = new DoctorDto
            {
                Id = doctor.Id,
                UserName = doctor.Username,
                Name = doctor.Name,
                Specialty = doctor.Specialty,
                PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email,
                IsActive = doctor.IsActive
            };

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctorDto);
        }

        // PUT: /api/doctors/{id}
        // Updating doctor details (excluding user login) is restricted to Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(int id, UpdateDoctorDto updateDoctorDto)
        {
            var doctor = await _context.Set<Doctor>().FindAsync(id);
            if (doctor == null) return NotFound();

            // Check if email change creates a conflict
            if (doctor.Email != updateDoctorDto.Email &&
                await _context.Set<Doctor>().AnyAsync(d => d.Email == updateDoctorDto.Email))
            {
                return BadRequest("Email already used by another doctor.");
            }

            doctor.Name = updateDoctorDto.Name;
            doctor.Specialty = updateDoctorDto.Specialty;
            doctor.PhoneNumber = updateDoctorDto.PhoneNumber;
            doctor.Email = updateDoctorDto.Email;
            doctor.IsActive = updateDoctorDto.IsActive; // Allow Admin to change active status

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/doctors/{id} (Soft Delete)
        // Restricted to Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Set<Doctor>().FindAsync(id);
            if (doctor == null) return NotFound();

            // Check if doctor has future scheduled appointments
            var hasScheduledAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == id && a.Status == "Scheduled" && a.AppointmentDate >= DateTime.Today);

            if (hasScheduledAppointments)
                return BadRequest("Cannot deactivate doctor with future scheduled appointments.");

            // Soft delete: set IsActive to false
            doctor.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchDoctors([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term is required");

            var patients = await _context.Set<Doctor>()
                .Where(d => d.Name.Contains(term) || d.PhoneNumber.Contains(term)) // Added phone number search
                .Select(p => new DoctorDto
                {
                    Id = p.Id,
                    UserName = p.Username,
                    Name = p.Name,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email
                })
                .ToListAsync();

            return Ok(patients);
        }
    }
}