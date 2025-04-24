using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class OneToOneDriverVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Drivers_DriverId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Drivers_DriverId1",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DriverId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DriverId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "Vehicles",
                newName: "modele");

            migrationBuilder.RenameColumn(
                name: "LicensePlate",
                table: "Vehicles",
                newName: "license_plate");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "Vehicles",
                newName: "driver_id");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Vehicles",
                newName: "couleur");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "Vehicles",
                newName: "marque");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Vehicles",
                newName: "vehicle_id");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Drivers",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Drivers",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Drivers",
                newName: "first_name");

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "Vehicles",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_driver_id",
                table: "Vehicles",
                column: "driver_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Drivers_driver_id",
                table: "Vehicles",
                column: "driver_id",
                principalTable: "Drivers",
                principalColumn: "driver_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Drivers_driver_id",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_driver_id",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "year",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "modele",
                table: "Vehicles",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "marque",
                table: "Vehicles",
                newName: "Brand");

            migrationBuilder.RenameColumn(
                name: "license_plate",
                table: "Vehicles",
                newName: "LicensePlate");

            migrationBuilder.RenameColumn(
                name: "driver_id",
                table: "Vehicles",
                newName: "DriverId");

            migrationBuilder.RenameColumn(
                name: "couleur",
                table: "Vehicles",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "vehicle_id",
                table: "Vehicles",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Drivers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Drivers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Drivers",
                newName: "FirstName");

            migrationBuilder.AddColumn<Guid>(
                name: "DriverId1",
                table: "Vehicles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverId",
                table: "Vehicles",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverId1",
                table: "Vehicles",
                column: "DriverId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Drivers_DriverId",
                table: "Vehicles",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "driver_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Drivers_DriverId1",
                table: "Vehicles",
                column: "DriverId1",
                principalTable: "Drivers",
                principalColumn: "driver_id");
        }
    }
}
