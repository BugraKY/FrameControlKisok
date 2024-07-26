using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrameControlKiosk.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReportDetailId",
                table: "CoordinateCheck",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "MarkSize",
                table: "Coordinate",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportDetailId",
                table: "CoordinateCheck");

            migrationBuilder.DropColumn(
                name: "MarkSize",
                table: "Coordinate");
        }
    }
}
