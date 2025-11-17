namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs

{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime Expires { get; set; }
    }
}