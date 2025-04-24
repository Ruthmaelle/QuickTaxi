using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class FixRideColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_AspNetUsers_PassengerId",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_DriverId",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Rates_RateId",
                table: "Rides");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Rides",
                newName: "statut");

            migrationBuilder.RenameColumn(
                name: "RideDate",
                table: "Rides",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "RateId",
                table: "Rides",
                newName: "tarif_applique");

            migrationBuilder.RenameColumn(
                name: "PickupLongitude",
                table: "Rides",
                newName: "pickup_longitude");

            migrationBuilder.RenameColumn(
                name: "PickupLatitude",
                table: "Rides",
                newName: "pickup_latitude");

            migrationBuilder.RenameColumn(
                name: "PickupAddress",
                table: "Rides",
                newName: "pickup_address");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Rides",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "PassengerId",
                table: "Rides",
                newName: "passenger_id");

            migrationBuilder.RenameColumn(
                name: "EstimatedPrice",
                table: "Rides",
                newName: "estimated_price");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "Rides",
                newName: "driver_id");

            migrationBuilder.RenameColumn(
                name: "DistanceKm",
                table: "Rides",
                newName: "distance_km");

            migrationBuilder.RenameColumn(
                name: "DestinationLongitude",
                table: "Rides",
                newName: "dropoff_longitude");

            migrationBuilder.RenameColumn(
                name: "DestinationLatitude",
                table: "Rides",
                newName: "dropoff_latitude");

            migrationBuilder.RenameColumn(
                name: "DestinationAddress",
                table: "Rides",
                newName: "dropoff_address");

            migrationBuilder.RenameColumn(
                name: "RideId",
                table: "Rides",
                newName: "ride_id");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_RateId",
                table: "Rides",
                newName: "IX_Rides_tarif_applique");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_PassengerId",
                table: "Rides",
                newName: "IX_Rides_passenger_id");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_DriverId",
                table: "Rides",
                newName: "IX_Rides_driver_id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Rates",
                newName: "heure_debut");

            migrationBuilder.RenameColumn(
                name: "Multiplier",
                table: "Rates",
                newName: "multiplicateur");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Rates",
                newName: "heure_fin");

            migrationBuilder.RenameColumn(
                name: "RateName",
                table: "Rates",
                newName: "nom_tarification");

            migrationBuilder.RenameColumn(
                name: "RateId",
                table: "Rates",
                newName: "id_tarification");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_AspNetUsers_passenger_id",
                table: "Rides",
                column: "passenger_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Drivers_driver_id",
                table: "Rides",
                column: "driver_id",
                principalTable: "Drivers",
                principalColumn: "driver_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Rates_tarif_applique",
                table: "Rides",
                column: "tarif_applique",
                principalTable: "Rates",
                principalColumn: "id_tarification",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_AspNetUsers_passenger_id",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_driver_id",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Rates_tarif_applique",
                table: "Rides");

            migrationBuilder.RenameColumn(
                name: "tarif_applique",
                table: "Rides",
                newName: "RateId");

            migrationBuilder.RenameColumn(
                name: "statut",
                table: "Rides",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "pickup_longitude",
                table: "Rides",
                newName: "PickupLongitude");

            migrationBuilder.RenameColumn(
                name: "pickup_latitude",
                table: "Rides",
                newName: "PickupLatitude");

            migrationBuilder.RenameColumn(
                name: "pickup_address",
                table: "Rides",
                newName: "PickupAddress");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "Rides",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "passenger_id",
                table: "Rides",
                newName: "PassengerId");

            migrationBuilder.RenameColumn(
                name: "estimated_price",
                table: "Rides",
                newName: "EstimatedPrice");

            migrationBuilder.RenameColumn(
                name: "dropoff_longitude",
                table: "Rides",
                newName: "DestinationLongitude");

            migrationBuilder.RenameColumn(
                name: "dropoff_latitude",
                table: "Rides",
                newName: "DestinationLatitude");

            migrationBuilder.RenameColumn(
                name: "dropoff_address",
                table: "Rides",
                newName: "DestinationAddress");

            migrationBuilder.RenameColumn(
                name: "driver_id",
                table: "Rides",
                newName: "DriverId");

            migrationBuilder.RenameColumn(
                name: "distance_km",
                table: "Rides",
                newName: "DistanceKm");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Rides",
                newName: "RideDate");

            migrationBuilder.RenameColumn(
                name: "ride_id",
                table: "Rides",
                newName: "RideId");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_tarif_applique",
                table: "Rides",
                newName: "IX_Rides_RateId");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_passenger_id",
                table: "Rides",
                newName: "IX_Rides_PassengerId");

            migrationBuilder.RenameIndex(
                name: "IX_Rides_driver_id",
                table: "Rides",
                newName: "IX_Rides_DriverId");

            migrationBuilder.RenameColumn(
                name: "multiplicateur",
                table: "Rates",
                newName: "Multiplier");

            migrationBuilder.RenameColumn(
                name: "heure_fin",
                table: "Rates",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "heure_debut",
                table: "Rates",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "nom_tarification",
                table: "Rates",
                newName: "RateName");

            migrationBuilder.RenameColumn(
                name: "id_tarification",
                table: "Rates",
                newName: "RateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_AspNetUsers_PassengerId",
                table: "Rides",
                column: "PassengerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Drivers_DriverId",
                table: "Rides",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "driver_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Rates_RateId",
                table: "Rides",
                column: "RateId",
                principalTable: "Rates",
                principalColumn: "RateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
