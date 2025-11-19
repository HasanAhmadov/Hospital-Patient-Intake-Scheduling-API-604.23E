using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Data.Migrations
{
    /// <inheritdoc />
    public partial class Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Doctor_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Specialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Doctor_PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Doctor_Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Symptoms = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsFollowUp = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "UserType", "Username" },
                values: new object[,]
                {
                    { 1, "$2a$11$SHe0o6p9b8o4fIkOpti/W.U6Jm/mFO61RT1ZVGmEDHqAQfzI8tCcW", "Admin", "User", "admin" },
                    { 2, "$2a$11$Et32zOFek1KeOY0922fxZebD0CCk1.XFXdCoZ7aUstXPOWAglLfbm", "Receptionist", "User", "receptionist" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Doctor_Email", "IsActive", "Doctor_Name", "PasswordHash", "Doctor_PhoneNumber", "Role", "Specialty", "UserType", "Username" },
                values: new object[] { 3, "hasan.ahmadov@xestexanam.az", true, "Dr. Hasan Ahmadov", "$2a$11$lkrIIvWua/cHhdYzD.0zTeL7KOjhI/W9aZIjlAoH0Gm1Ofh3./E3G", "+994777777777", "Doctor", "Cardiology", "Doctor", "hasan" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "Age", "CreatedAt", "Email", "Name", "PasswordHash", "PhoneNumber", "Role", "Symptoms", "UpdatedAt", "UserType", "Username" },
                values: new object[] { 4, "Ahmadli", 19, new DateTime(2025, 11, 19, 18, 2, 34, 679, DateTimeKind.Utc).AddTicks(8816), "sevinc@gmail.com", "Sevinc Abbasova", "$2a$11$piebLk58yPt48p.36YS2bu5UorEKRocXt2sXo0wfaY11cXuuqGjA2", "+994555555555", "Patient", "Headache, nausea, dizziness", new DateTime(2025, 11, 19, 18, 2, 34, 679, DateTimeKind.Utc).AddTicks(8821), "Patient", "sevinc" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
