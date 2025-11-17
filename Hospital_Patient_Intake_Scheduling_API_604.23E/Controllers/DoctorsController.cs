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
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specialty = d.Specialty,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Where(d => d.Id == id && d.IsActive)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDto>> CreateDoctor(CreateDoctorDto createDoctorDto)
        {
            var doctor = new Doctor
            {
                Name = createDoctorDto.Name,
                Specialty = createDoctorDto.Specialty,
                PhoneNumber = createDoctorDto.PhoneNumber,
                Email = createDoctorDto.Email,
                IsActive = true
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var doctorDto = new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Specialty = doctor.Specialty,
                PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email,
                IsActive = doctor.IsActive
            };

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctorDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(int id, CreateDoctorDto updateDoctorDto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.Name = updateDoctorDto.Name;
            doctor.Specialty = updateDoctorDto.Specialty;
            doctor.PhoneNumber = updateDoctorDto.PhoneNumber;
            doctor.Email = updateDoctorDto.Email;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            // Check if doctor has appointments
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == id && a.Status == "Scheduled");
            if (hasAppointments)
                return BadRequest("Cannot delete doctor with scheduled appointments");

            doctor.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}