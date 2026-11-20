using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nautivo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CourseCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "CourseSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            // Dotąd postęp liczył się z dat. Zajęcia z przeszłości i kursy po terminie końca uznajemy za ukończone.
            migrationBuilder.Sql("""UPDATE "CourseSessions" SET "CompletedAt" = ("Date"::timestamp AT TIME ZONE 'UTC') WHERE "Date" < CURRENT_DATE AND "CompletedAt" IS NULL""");
            migrationBuilder.Sql("""UPDATE "Courses" SET "CompletedAt" = ("EndDate"::timestamp AT TIME ZONE 'UTC') WHERE "EndDate" < CURRENT_DATE AND "Status" = 'Planned' AND "CompletedAt" IS NULL""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "CourseSessions");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Courses");
        }
    }
}
