using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // Removed: private readonly IAppointmentService _appointmentService;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
            // Removed: _appointmentService = appointmentService;
        }

        // GET: /api/appointments (Returns all appointments)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.Name,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    IsFollowUp = a.IsFollowUp
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: /api/appointments/{id} 
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Id == id)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.Name,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    IsFollowUp = a.IsFollowUp
                })
                .FirstOrDefaultAsync();

            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        // GET: /api/appointments/today
        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetTodayAppointments()
        {
            var today = DateTime.Today;

            // Logic implemented directly: Filter appointments for the current date
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate.Date == today)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.Name,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    IsFollowUp = a.IsFollowUp
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: /api/appointments/availability
        // Since full availability (based on doctor shifts) required the removed service,
        // this method now returns the SCHEDULED slots, which helps users see what is unavailable.
        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAvailability(
    [FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            // Get all appointments for the doctor on the given date
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .ToListAsync();

            // Convert to TimeSlotDto with proper availability status
            var scheduledSlots = appointments.Select(a => new TimeSlotDto
            {
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Date = a.AppointmentDate,
                IsAvailable = a.Status == "Cancelled" // Only cancelled appointments are considered "available"
            }).ToList();

            if (scheduledSlots is null || scheduledSlots.Count == 0)
            {
                return NotFound("No appointments found for the specified doctor and date.");
            }

            return Ok(scheduledSlots);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto createAppointmentDto)
        {
            // Logic implemented directly: Check for slot availability (overlap)
            var overlapExists = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == createAppointmentDto.DoctorId &&
                    a.AppointmentDate.Date == createAppointmentDto.AppointmentDate.Date &&
                    // Overlap condition: (A_start < B_end) AND (B_start < A_end)
                    a.StartTime < createAppointmentDto.EndTime &&
                    createAppointmentDto.StartTime < a.EndTime &&
                    // Do not count cancelled appointments as overlap
                    a.Status != "Cancelled");


            if (overlapExists)
                return BadRequest("The selected time slot is not available (overlap detected).");

            // Validate patient and doctor exist
            var patient = await _context.Patients.FindAsync(createAppointmentDto.PatientId);
            var doctor = await _context.Doctors.FindAsync(createAppointmentDto.DoctorId);

            if (patient == null || doctor == null)
                return BadRequest("Invalid patient or doctor");

            var appointment = new Appointment
            {
                PatientId = createAppointmentDto.PatientId,
                DoctorId = createAppointmentDto.DoctorId,
                AppointmentDate = createAppointmentDto.AppointmentDate.Date,
                StartTime = createAppointmentDto.StartTime,
                EndTime = createAppointmentDto.EndTime,
                Status = "Scheduled",
                Notes = createAppointmentDto.Notes,
                IsFollowUp = createAppointmentDto.IsFollowUp,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = patient.Name,
                DoctorId = appointment.DoctorId,
                DoctorName = doctor.Name,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                Notes = appointment.Notes,
                IsFollowUp = appointment.IsFollowUp
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentDto);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Receptionist")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}