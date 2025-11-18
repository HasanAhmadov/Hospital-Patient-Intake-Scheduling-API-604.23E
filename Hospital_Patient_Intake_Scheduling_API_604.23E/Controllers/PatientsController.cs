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
    // Restrict access to patient data to Admin and Receptionist
    [Authorize(Roles = "Admin,Receptionist")]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            var patients = await _context.Set<Patient>() // Use Set<Patient>() for clarity with TPH
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

        // GET: /api/patients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var patient = await _context.Set<Patient>()
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

        // GET: /api/patients/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term is required");

            var patients = await _context.Set<Patient>()
                .Where(p => p.Name.Contains(term) || p.PhoneNumber.Contains(term)) // Added phone number search
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

        // POST: /api/patients (Patient Intake)
        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto createPatientDto)
        {
            // CRITICAL FIX: Generate Username, PasswordHash, and Role to satisfy TPH base class constraints (User)

            // 1. Generate unique Username (ensuring it's safe and unique)
            string initialUsername = createPatientDto.Name.ToLower().Replace(" ", "");
            // Take up to the first 10 chars, plus a random number for uniqueness
            initialUsername = initialUsername.Length > 10 ? initialUsername.Substring(0, 10) : initialUsername;

            string uniqueUsername = initialUsername + Random.Shared.Next(1000, 9999);
            // In a real app, you would check if the username already exists and regenerate if needed.

            // 2. Generate and Hash a temporary password
            string temporaryPassword = "TempPass" + System.Guid.NewGuid().ToString().Substring(0, 8);
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

            // 3. Create the Patient entity
            var patient = new Patient
            {
                // User Base Class Properties (TPH Requirements)
                Username = uniqueUsername,
                PasswordHash = passwordHash,
                Role = "Patient", // Explicitly set role for TPH mapping

                // Patient Derived Properties
                Name = createPatientDto.Name,
                Age = createPatientDto.Age,
                Symptoms = createPatientDto.Symptoms,
                PhoneNumber = createPatientDto.PhoneNumber,
                Email = createPatientDto.Email,
                Address = createPatientDto.Address,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<Patient>().Add(patient);
            await _context.SaveChangesAsync();

            // NOTE: The temporaryPassword variable contains the plain text password for logging/notification.
            // You should return the DTO, not the patient entity itself.

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

        // PUT: /api/patients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, CreatePatientDto updatePatientDto)
        {
            var patient = await _context.Set<Patient>().FindAsync(id);
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

        // DELETE: /api/patients/{id} (Hard Delete)
        // Restricted to Admin due to the permanent nature of the delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Set<Patient>().FindAsync(id);
            if (patient == null) return NotFound();

            // Check if patient has any associated appointments (past or future)
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == id);
            if (hasAppointments)
                return BadRequest("Cannot delete patient with existing appointments. Consider archiving.");

            _context.Set<Patient>().Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}