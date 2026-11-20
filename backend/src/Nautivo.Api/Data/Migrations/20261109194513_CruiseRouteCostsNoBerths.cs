using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nautivo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CruiseRouteCostsNoBerths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CruiseId_Berth",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Berth",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Skipper",
                table: "Cruises");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Cruises",
                newName: "DeparturePort");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalPort",
                table: "Cruises",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoutePoints",
                table: "Cruises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "YachtLengthMeters",
                table: "Cruises",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YachtModel",
                table: "Cruises",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YachtName",
                table: "Cruises",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Dotychczasowa trasa była tekstem „Port, Port, Port”. Zamieniamy ją na listę punktów bez współrzędnych.
            // Gdy trasa była pusta, startem i metą jest dotychczasowa miejscowość startu.
            migrationBuilder.Sql("""
                UPDATE "Cruises" c SET "RoutePoints" = COALESCE(
                    (SELECT jsonb_agg(jsonb_build_object('Name', trim(x.port), 'Lat', NULL, 'Lng', NULL) ORDER BY x.n)
                     FROM unnest(string_to_array(c."Route", ',')) WITH ORDINALITY AS x(port, n)
                     WHERE trim(x.port) <> ''),
                    '[]'::jsonb);
                UPDATE "Cruises" SET "RoutePoints" = jsonb_build_array(
                        jsonb_build_object('Name', "DeparturePort", 'Lat', NULL, 'Lng', NULL),
                        jsonb_build_object('Name', "DeparturePort", 'Lat', NULL, 'Lng', NULL))
                    WHERE jsonb_array_length("RoutePoints") < 2 AND "DeparturePort" <> '';
                UPDATE "Cruises" SET
                    "DeparturePort" = COALESCE("RoutePoints"->0->>'Name', "DeparturePort"),
                    "ArrivalPort" = COALESCE("RoutePoints" -> -1 ->> 'Name', '');
                UPDATE "Cruises" c SET "YachtName" = v."Name", "YachtModel" = NULLIF(v."Model", ''), "YachtLengthMeters" = NULLIF(v."LengthMeters", 0)
                    FROM "Vessels" v WHERE v."Id" = c."VesselId";
                -- Bez listy rezerwowej: zapisy rezerwowe nie mają wpłat (wpłatę przyjmowaliśmy tylko od potwierdzonych).
                DELETE FROM "Enrollments" e WHERE e."Status" = 'Waitlist'
                    AND NOT EXISTS (SELECT 1 FROM "Payments" p WHERE p."EnrollmentId" = e."Id");
                """);

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Cruises");

            migrationBuilder.CreateTable(
                name: "CruiseCosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CruiseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CruiseCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CruiseCosts_Cruises_CruiseId",
                        column: x => x.CruiseId,
                        principalTable: "Cruises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CruiseCosts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CruiseCosts_CruiseId",
                table: "CruiseCosts",
                column: "CruiseId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseCosts_OrganizationId_CruiseId",
                table: "CruiseCosts",
                columns: new[] { "OrganizationId", "CruiseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CruiseCosts");

            migrationBuilder.DropColumn(
                name: "ArrivalPort",
                table: "Cruises");

            migrationBuilder.DropColumn(
                name: "RoutePoints",
                table: "Cruises");

            migrationBuilder.DropColumn(
                name: "YachtLengthMeters",
                table: "Cruises");

            migrationBuilder.DropColumn(
                name: "YachtModel",
                table: "Cruises");

            migrationBuilder.DropColumn(
                name: "YachtName",
                table: "Cruises");

            migrationBuilder.RenameColumn(
                name: "DeparturePort",
                table: "Cruises",
                newName: "Location");

            migrationBuilder.AddColumn<string>(
                name: "Berth",
                table: "Enrollments",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Cruises",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Skipper",
                table: "Cruises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CruiseId_Berth",
                table: "Enrollments",
                columns: new[] { "CruiseId", "Berth" },
                unique: true,
                filter: "\"Status\" = 'Confirmed' AND \"Berth\" IS NOT NULL");
        }
    }
}
