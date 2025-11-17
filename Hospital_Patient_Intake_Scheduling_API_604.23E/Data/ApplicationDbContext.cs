using Microsoft.EntityFrameworkCore;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure DateTime properties
            modelBuilder.Entity<Patient>()
                .Property(p => p.CreatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Patient>()
                .Property(p => p.UpdatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.CreatedAt)
                .HasColumnType("datetime2");

            // Configure AppointmentDate as datetime2
            modelBuilder.Entity<Appointment>()
                .Property(a => a.AppointmentDate)
                .HasColumnType("datetime2");

            // Configure TimeSpan properties as time
            modelBuilder.Entity<Appointment>()
                .Property(a => a.StartTime)
                .HasColumnType("time");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.EndTime)
                .HasColumnType("time");

            modelBuilder.Entity<DoctorAvailability>()
                .Property(a => a.StartTime)
                .HasColumnType("time");

            modelBuilder.Entity<DoctorAvailability>()
                .Property(a => a.EndTime)
                .HasColumnType("time");

            // Configure string properties with proper lengths
            modelBuilder.Entity<Patient>()
                .Property(p => p.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Symptoms)
                .HasMaxLength(500);

            modelBuilder.Entity<Patient>()
                .Property(p => p.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Email)
                .HasMaxLength(200);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Address)
                .HasMaxLength(500);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Specialty)
                .HasMaxLength(100);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Email)
                .HasMaxLength(200);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasMaxLength(20);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Notes)
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(20);

            // REMOVED: All unique index and computed column code
            // We'll handle double-booking in service layer

            // Seed initial data
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Nuran Tagiyev",
                    Specialty = "General Medicine",
                    PhoneNumber = "+9945555555555",
                    Email = "nuran.taghiyev@xestexanam.az"
                },
                new Doctor
                {
                    Id = 2,
                    Name = "Dr. Hasan Ahmadov",
                    Specialty = "Cardiology",
                    PhoneNumber = "+994777777777",
                    Email = "hasan.ahmadov@xestexanam.az"
                },
                new Doctor
                {
                    Id = 3,
                    Name = "Dr. Sevinc Abbasova",
                    Specialty = "Pediatrics",
                    PhoneNumber = "+994999999999",
                    Email = "sevinc.abbasova@xestexanam.az"
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Username = "reception",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Receptionist"
                }
            );
        }
    }
}