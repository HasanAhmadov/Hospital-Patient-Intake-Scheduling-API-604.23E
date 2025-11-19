using System.ComponentModel.DataAnnotations;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // Receptionist, Doctor, Admin
    }
}