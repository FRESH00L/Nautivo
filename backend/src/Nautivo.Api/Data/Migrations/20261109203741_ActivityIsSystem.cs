using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nautivo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActivityIsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "ContactActivities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Wpisy o zmianie zgody dodane przed tą migracją też są systemowe.
            migrationBuilder.Sql("""UPDATE "ContactActivities" SET "IsSystem" = TRUE WHERE "Text" LIKE 'Odnotowano%zgod%'""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "ContactActivities");
        }
    }
}
