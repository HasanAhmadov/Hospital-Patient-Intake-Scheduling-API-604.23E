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
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

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

        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetTodayAppointments()
        {
            var appointments = await _appointmentService.GetTodaysAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAvailability(
            [FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto createAppointmentDto)
        {
            // Check if slot is available
            var isAvailable = await _appointmentService.IsSlotAvailableAsync(
                createAppointmentDto.DoctorId,
                createAppointmentDto.AppointmentDate,
                createAppointmentDto.StartTime,
                createAppointmentDto.EndTime);

            if (!isAvailable)
                return BadRequest("The selected time slot is not available");

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

            return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, appointmentDto);
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