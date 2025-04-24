using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class FixDriverTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Drivers",
                newName: "statut");

            migrationBuilder.RenameColumn(
                name: "AverageRating",
                table: "Drivers",
                newName: "note_moyenne");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "Drivers",
                newName: "driver_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drivers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Drivers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "UserName",
                keyValue: null,
                column: "UserName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "statut",
                table: "Drivers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "note_moyenne",
                table: "Drivers",
                newName: "AverageRating");

            migrationBuilder.RenameColumn(
                name: "driver_id",
                table: "Drivers",
                newName: "DriverId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
