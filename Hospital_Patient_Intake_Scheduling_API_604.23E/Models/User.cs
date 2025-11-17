namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Receptionist, Doctor, Admin
    }
}