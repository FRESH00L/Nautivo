using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nautivo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Courses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nowy moduł włączamy istniejącym organizacjom, tak jak nowym (domyślnie wszystkie moduły).
            migrationBuilder.Sql("""UPDATE "Organizations" SET "EnabledModules" = array_append("EnabledModules", 'szkolenia') WHERE NOT ('szkolenia' = ANY("EnabledModules"))""");

            migrationBuilder.AddColumn<string[]>(
                name: "ManualQualifications",
                table: "Participants",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AlterColumn<Guid>(
                name: "CruiseId",
                table: "Enrollments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "Enrollments",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Enrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "DocumentsDelivered",
                table: "Enrollments",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "ExamResult",
                table: "Enrollments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "CruiseId",
                table: "CruiseCosts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "CruiseCosts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Instructor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Deposit = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    BalanceDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExamInfo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequiredDocuments = table.Column<string[]>(type: "text[]", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseMaterials_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseMaterials_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PassScore = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseTests_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseTests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseAttendance",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAttendance", x => new { x.SessionId, x.EnrollmentId });
                    table.ForeignKey(
                        name: "FK_CourseAttendance_CourseSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CourseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseAttendance_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseAttendance_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseTestResults",
                columns: table => new
                {
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTestResults", x => new { x.TestId, x.EnrollmentId });
                    table.ForeignKey(
                        name: "FK_CourseTestResults_CourseTests_TestId",
                        column: x => x.TestId,
                        principalTable: "CourseTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseTestResults_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseTestResults_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId_ParticipantId",
                table: "Enrollments",
                columns: new[] { "CourseId", "ParticipantId" },
                unique: true,
                filter: "\"Status\" <> 'Cancelled'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_CruiseOrCourse",
                table: "Enrollments",
                sql: "(\"CruiseId\" IS NULL) <> (\"CourseId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseCosts_CourseId",
                table: "CruiseCosts",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseCosts_OrganizationId_CourseId",
                table: "CruiseCosts",
                columns: new[] { "OrganizationId", "CourseId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CruiseCosts_CruiseOrCourse",
                table: "CruiseCosts",
                sql: "(\"CruiseId\" IS NULL) <> (\"CourseId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_EnrollmentId",
                table: "CourseAttendance",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_OrganizationId",
                table: "CourseAttendance",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseMaterials_CourseId",
                table: "CourseMaterials",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseMaterials_OrganizationId_CourseId",
                table: "CourseMaterials",
                columns: new[] { "OrganizationId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_OrganizationId_StartDate",
                table: "Courses",
                columns: new[] { "OrganizationId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_CourseId",
                table: "CourseSessions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_OrganizationId_CourseId_Date",
                table: "CourseSessions",
                columns: new[] { "OrganizationId", "CourseId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTestResults_EnrollmentId",
                table: "CourseTestResults",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTestResults_OrganizationId",
                table: "CourseTestResults",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTests_CourseId",
                table: "CourseTests",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTests_OrganizationId_CourseId",
                table: "CourseTests",
                columns: new[] { "OrganizationId", "CourseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CruiseCosts_Courses_CourseId",
                table: "CruiseCosts",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CruiseCosts_Courses_CourseId",
                table: "CruiseCosts");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseAttendance");

            migrationBuilder.DropTable(
                name: "CourseMaterials");

            migrationBuilder.DropTable(
                name: "CourseTestResults");

            migrationBuilder.DropTable(
                name: "CourseSessions");

            migrationBuilder.DropTable(
                name: "CourseTests");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId_ParticipantId",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_CruiseOrCourse",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_CruiseCosts_CourseId",
                table: "CruiseCosts");

            migrationBuilder.DropIndex(
                name: "IX_CruiseCosts_OrganizationId_CourseId",
                table: "CruiseCosts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CruiseCosts_CruiseOrCourse",
                table: "CruiseCosts");

            migrationBuilder.DropColumn(
                name: "ManualQualifications",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "DocumentsDelivered",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "ExamResult",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CruiseCosts");

            migrationBuilder.AlterColumn<Guid>(
                name: "CruiseId",
                table: "Enrollments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CruiseId",
                table: "CruiseCosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
