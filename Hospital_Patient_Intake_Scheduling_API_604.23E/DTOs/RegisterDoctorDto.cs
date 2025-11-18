using System.ComponentModel.DataAnnotations;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs

{
    public class RegisterDoctorDto
    {
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Specialty { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
    }
}