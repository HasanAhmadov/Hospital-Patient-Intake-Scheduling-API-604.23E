using Microsoft.EntityFrameworkCore;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Data;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var slots = new List<TimeSlotDto>();
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status != "Cancelled")
                .ToListAsync();

            // Generate 30-minute slots from 8 AM to 5 PM
            var startTime = new TimeSpan(8, 0, 0);
            var endTime = new TimeSpan(17, 0, 0);
            var slotDuration = TimeSpan.FromMinutes(30);

            for (var current = startTime; current < endTime; current = current.Add(slotDuration))
            {
                var slotEnd = current.Add(slotDuration);
                var isAvailable = !appointments.Any(a =>
                    a.StartTime < slotEnd && a.EndTime > current);

                slots.Add(new TimeSlotDto
                {
                    StartTime = current,
                    EndTime = slotEnd,
                    IsAvailable = isAvailable
                });
            }

            return slots;
        }

        public async Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var conflictingAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                              a.AppointmentDate.Date == date.Date &&
                              a.Status != "Cancelled" &&
                              a.StartTime < endTime &&
                              a.EndTime > startTime);

            return !conflictingAppointment;
        }

        public async Task<List<AppointmentDto>> GetTodaysAppointmentsAsync()
        {
            var today = DateTime.Today;
            return await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today && a.Status == "Scheduled")
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
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }
    }
}