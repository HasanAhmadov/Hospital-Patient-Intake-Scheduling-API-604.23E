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
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            var patients = await _context.Patients
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Age = p.Age,
                    Symptoms = p.Symptoms,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Address = p.Address
                })
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var patient = await _context.Patients
                .Where(p => p.Id == id)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Age = p.Age,
                    Symptoms = p.Symptoms,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Address = p.Address
                })
                .FirstOrDefaultAsync();

            if (patient == null) return NotFound();

            return patient;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term is required");

            var patients = await _context.Patients
                .Where(p => p.Name.Contains(term))
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Age = p.Age,
                    Symptoms = p.Symptoms,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Address = p.Address
                })
                .ToListAsync();

            return Ok(patients);
        }

        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto createPatientDto)
        {
            var patient = new Patient
            {
                Name = createPatientDto.Name,
                Age = createPatientDto.Age,
                Symptoms = createPatientDto.Symptoms,
                PhoneNumber = createPatientDto.PhoneNumber,
                Email = createPatientDto.Email,
                Address = createPatientDto.Address,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            var patientDto = new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Age = patient.Age,
                Symptoms = patient.Symptoms,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address
            };

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patientDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, CreatePatientDto updatePatientDto)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.Name = updatePatientDto.Name;
            patient.Age = updatePatientDto.Age;
            patient.Symptoms = updatePatientDto.Symptoms;
            patient.PhoneNumber = updatePatientDto.PhoneNumber;
            patient.Email = updatePatientDto.Email;
            patient.Address = updatePatientDto.Address;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            // Check if patient has appointments
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == id);
            if (hasAppointments)
                return BadRequest("Cannot delete patient with existing appointments");

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}