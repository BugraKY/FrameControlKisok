using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrameControlKiosk.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StationLine",
                table: "ReportDetail");

            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "ReportDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StationId",
                table: "ReportDetail");

            migrationBuilder.AddColumn<long>(
                name: "StationLine",
                table: "ReportDetail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
