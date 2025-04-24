using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class FixDriverForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_AspNetUsers_UserId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Drivers",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "IsApproved",
                table: "Drivers",
                newName: "is_approved");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Drivers",
                newName: "created_at");

            migrationBuilder.AlterColumn<bool>(
                name: "is_approved",
                table: "Drivers",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_user_id",
                table: "Drivers",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_AspNetUsers_user_id",
                table: "Drivers",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_AspNetUsers_user_id",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_user_id",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Drivers",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "is_approved",
                table: "Drivers",
                newName: "IsApproved");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Drivers",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "Drivers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_AspNetUsers_UserId",
                table: "Drivers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
