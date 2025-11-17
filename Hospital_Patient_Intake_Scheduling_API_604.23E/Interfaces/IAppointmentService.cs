using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<List<AppointmentDto>> GetTodaysAppointmentsAsync();
    }
}