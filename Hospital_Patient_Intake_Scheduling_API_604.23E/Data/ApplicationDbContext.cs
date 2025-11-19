using Microsoft.EntityFrameworkCore;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;
using BCrypt.Net;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enable legacy timestamp behavior for PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // 🛑 TPH CONFIGURATION
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Doctor>("Doctor")
                .HasValue<Patient>("Patient");

            // --- FOREIGN KEY CASCADE FIX ---
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- PROPERTY CONFIGURATION (PostgreSQL Compatible) ---

            // User properties
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(255);
            modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(20);

            // Patient properties
            modelBuilder.Entity<Patient>().Property(p => p.Name).HasMaxLength(100);
            modelBuilder.Entity<Patient>().Property(p => p.Symptoms).HasMaxLength(500);
            modelBuilder.Entity<Patient>().Property(p => p.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<Patient>().Property(p => p.Email).HasMaxLength(200);
            modelBuilder.Entity<Patient>().Property(p => p.Address).HasMaxLength(500);
            modelBuilder.Entity<Patient>().Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Patient>().Property(p => p.UpdatedAt).HasDefaultValueSql("NOW()");

            // Doctor properties
            modelBuilder.Entity<Doctor>().Property(d => d.Name).HasMaxLength(100);
            modelBuilder.Entity<Doctor>().Property(d => d.Specialty).HasMaxLength(100);
            modelBuilder.Entity<Doctor>().Property(d => d.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<Doctor>().Property(d => d.Email).HasMaxLength(200);

            // 🆕 APPOINTMENT PROPERTIES - Fix datetime2 and interval issues
            modelBuilder.Entity<Appointment>().Property(a => a.AppointmentDate).HasColumnType("date"); // Use PostgreSQL date type
            modelBuilder.Entity<Appointment>().Property(a => a.StartTime).HasColumnType("time"); // Use time instead of interval
            modelBuilder.Entity<Appointment>().Property(a => a.EndTime).HasColumnType("time"); // Use time instead of interval
            modelBuilder.Entity<Appointment>().Property(a => a.Status).HasMaxLength(20);
            modelBuilder.Entity<Appointment>().Property(a => a.Notes).HasMaxLength(500);
            modelBuilder.Entity<Appointment>().Property(a => a.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("NOW()");

            // --- SEEDING ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Username = "receptionist",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Receptionist"
                }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 3,
                    Username = "hasan",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Doctor",
                    Name = "Dr. Hasan Ahmadov",
                    Specialty = "Cardiology",
                    PhoneNumber = "+994777777777",
                    Email = "hasan.ahmadov@xestexanam.az",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 4,
                    Username = "sevinc",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Patient",
                    Name = "Sevinc Abbasova",
                    Age = 19,
                    Symptoms = "Headache, nausea, dizziness",
                    PhoneNumber = "+994555555555",
                    Email = "sevinc@gmail.com",
                    Address = "Ahmadli",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}