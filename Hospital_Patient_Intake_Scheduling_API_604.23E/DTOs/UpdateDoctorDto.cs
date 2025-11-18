using System.ComponentModel.DataAnnotations;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs

{
    public class UpdateDoctorDto
    {
        [Required] public string Name { get; set; }
        [Required] public string Specialty { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}