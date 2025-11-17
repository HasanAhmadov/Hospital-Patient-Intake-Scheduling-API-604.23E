namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs

{
    public class CreateAppointmentDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Notes { get; set; }
        public bool IsFollowUp { get; set; }
    }
}