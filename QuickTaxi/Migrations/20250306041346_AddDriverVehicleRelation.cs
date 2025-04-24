using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverVehicleRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "insurance_document_url",
                table: "Vehicles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "license_document_url",
                table: "Vehicles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "vehicle_registration_number",
                table: "Vehicles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "insurance_document_url",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "license_document_url",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "vehicle_registration_number",
                table: "Vehicles");
        }
    }
}
