using System.ComponentModel.DataAnnotations;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Models
{
    public class Patient : User
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 150)]
        public int Age { get; set; }

        [StringLength(500)]
        public string Symptoms { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<Appointment> Appointments { get; set; }
    }
}   