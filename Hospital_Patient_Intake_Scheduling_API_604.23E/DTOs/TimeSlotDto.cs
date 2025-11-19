namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs

{
    public class TimeSlotDto
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}