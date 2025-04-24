using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class FixRideReviewRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Rides_RideId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_RideId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RideId1",
                table: "Reviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RideId1",
                table: "Reviews",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RideId1",
                table: "Reviews",
                column: "RideId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Rides_RideId1",
                table: "Reviews",
                column: "RideId1",
                principalTable: "Rides",
                principalColumn: "ride_id");
        }
    }
}
