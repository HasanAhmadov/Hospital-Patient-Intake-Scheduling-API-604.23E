using System.ComponentModel.DataAnnotations;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Models
{
    public class Doctor : User
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<Appointment> Appointments { get; set; }
    }
}