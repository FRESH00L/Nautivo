using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nautivo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Charters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Istniejące organizacje dostają nowy moduł, tak jak przy szkoleniach.
            migrationBuilder.Sql("""UPDATE "Organizations" SET "EnabledModules" = array_append("EnabledModules", 'czartery') WHERE NOT ('czartery' = ANY("EnabledModules"))""");

            migrationBuilder.AddColumn<decimal>(
                name: "CharterDayRate",
                table: "Vessels",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Checklist",
                table: "Vessels",
                type: "jsonb",
                nullable: true,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Yacht");

            migrationBuilder.CreateTable(
                name: "Charters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DayRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charters", x => x.Id);
                    table.CheckConstraint("CK_Charters_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_Charters_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charters_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charters_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Charters_OrganizationId_StartDate",
                table: "Charters",
                columns: new[] { "OrganizationId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Charters_OrganizationId_VesselId_StartDate",
                table: "Charters",
                columns: new[] { "OrganizationId", "VesselId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Charters_ParticipantId",
                table: "Charters",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Charters_VesselId",
                table: "Charters",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Charters");

            migrationBuilder.DropColumn(
                name: "CharterDayRate",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Checklist",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Vessels");
        }
    }
}
