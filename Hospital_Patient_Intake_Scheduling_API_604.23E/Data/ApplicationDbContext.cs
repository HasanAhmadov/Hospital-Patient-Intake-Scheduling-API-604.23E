using Microsoft.EntityFrameworkCore;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Models;
using BCrypt.Net; // Assuming this is needed for the HashPassword calls

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- Core DbSets ---
        // Base class DbSet (required for TPH configuration)
        public DbSet<User> Users { get; set; }

        // Derived class DbSets (REQUIRED for strong-typed querying)
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        // Other model DbSets
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🛑 TPH CONFIGURATION: All user data in one 'Users' table
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Discriminator") // Column to distinguish the type
                .HasValue<User>("User")           // Base User (e.g., Admin, Receptionist)
                .HasValue<Doctor>("Doctor")       // Doctor type
                .HasValue<Patient>("Patient");    // Patient type

            // --- FOREIGN KEY CASCADE FIX (Resolves SqlException: Multiple Cascade Path) ---

            // Appointment-Patient relationship: We can keep CASCADE here.
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade); // If patient is deleted, their appointments should be too.

            // Appointment-Doctor relationship: CRITICAL FIX - MUST be RESTRICT to break the cycle.
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Ensures Doctor deletion doesn't cascade, preventing the cycle error.

            // --- PROPERTY CONFIGURATION ---

            // User properties (applied to the single Users table)
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(255);
            modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(20);

            // Patient properties
            modelBuilder.Entity<Patient>().Property(p => p.Name).HasMaxLength(100);
            modelBuilder.Entity<Patient>().Property(p => p.Symptoms).HasMaxLength(500);
            modelBuilder.Entity<Patient>().Property(p => p.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<Patient>().Property(p => p.Email).HasMaxLength(200);
            modelBuilder.Entity<Patient>().Property(p => p.Address).HasMaxLength(500);
            modelBuilder.Entity<Patient>().Property(p => p.CreatedAt).HasColumnType("datetime2");
            modelBuilder.Entity<Patient>().Property(p => p.UpdatedAt).HasColumnType("datetime2");

            // Doctor properties
            modelBuilder.Entity<Doctor>().Property(d => d.Name).HasMaxLength(100);
            modelBuilder.Entity<Doctor>().Property(d => d.Specialty).HasMaxLength(100);
            modelBuilder.Entity<Doctor>().Property(d => d.PhoneNumber).HasMaxLength(20);
            modelBuilder.Entity<Doctor>().Property(d => d.Email).HasMaxLength(200);

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

            // --- SEEDING ---
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
                    Email = "hasan.ahmadov@xestexanam.az"
                }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient {
                    Id = 4,
                    Username = "sevinc",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Patient",

                    Name = "Sevinc Abbasova",
                    Age = 19,
                    Symptoms = "Headache, nausea, dizziness",
                    PhoneNumber = "+994555555555",
                    Email = "sevinc@gmail.com",
                    Address = "Ahmadli"
                }   
             );

        }
    }
}