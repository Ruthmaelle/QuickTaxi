using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDriverSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverId1",
                table: "Vehicles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "Drivers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_birth",
                table: "Drivers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "insurance_document_url",
                table: "Drivers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "license_document_url",
                table: "Drivers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "license_expiration",
                table: "Drivers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "license_number",
                table: "Drivers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "Drivers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "vehicle_registration_number",
                table: "Drivers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverId1",
                table: "Vehicles",
                column: "DriverId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Drivers_DriverId1",
                table: "Vehicles",
                column: "DriverId1",
                principalTable: "Drivers",
                principalColumn: "driver_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Drivers_DriverId1",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DriverId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "address",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "insurance_document_url",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "license_document_url",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "license_expiration",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "license_number",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "vehicle_registration_number",
                table: "Drivers");
        }
    }
}
