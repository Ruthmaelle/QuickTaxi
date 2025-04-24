using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickTaxi.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Rides_RideId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Rides_RideId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_AspNetUsers_UserId",
                table: "Rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Rides_RideId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Rewards",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RideId",
                table: "Rewards",
                newName: "ride_id");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Rewards",
                newName: "points_earned");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Rewards",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Rewards",
                newName: "reward_id");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_UserId",
                table: "Rewards",
                newName: "IX_Rewards_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_RideId",
                table: "Rewards",
                newName: "IX_Rewards_ride_id");

            migrationBuilder.RenameColumn(
                name: "RideId",
                table: "Reviews",
                newName: "id_reservation");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Reviews",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "Reviews",
                newName: "commentaire");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "Reviews",
                newName: "id_evaluation");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_RideId",
                table: "Reviews",
                newName: "IX_Reviews_id_reservation");

            migrationBuilder.RenameColumn(
                name: "RideId",
                table: "Payments",
                newName: "id_reservation");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "date_paiement");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "Payments",
                newName: "methode");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "montant");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "Payments",
                newName: "id_paiement");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_RideId",
                table: "Payments",
                newName: "IX_Payments_id_reservation");

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "Rides",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_date",
                table: "Rides",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "reward_id",
                table: "Rewards",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "total_points",
                table: "Rewards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "note",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<Guid>(
                name: "RideId1",
                table: "Reviews",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Reviews",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_paid",
                table: "Payments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RideId1",
                table: "Reviews",
                column: "RideId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Rides_id_reservation",
                table: "Payments",
                column: "id_reservation",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Rides_RideId1",
                table: "Reviews",
                column: "RideId1",
                principalTable: "Rides",
                principalColumn: "ride_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Rides_id_reservation",
                table: "Reviews",
                column: "id_reservation",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_AspNetUsers_user_id",
                table: "Rewards",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Rides_ride_id",
                table: "Rewards",
                column: "ride_id",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Rides_id_reservation",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Rides_RideId1",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Rides_id_reservation",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_AspNetUsers_user_id",
                table: "Rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Rides_ride_id",
                table: "Rewards");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_RideId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "scheduled_date",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "total_points",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "RideId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "is_paid",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Rewards",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ride_id",
                table: "Rewards",
                newName: "RideId");

            migrationBuilder.RenameColumn(
                name: "points_earned",
                table: "Rewards",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Rewards",
                newName: "LastUpdated");

            migrationBuilder.RenameColumn(
                name: "reward_id",
                table: "Rewards",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_user_id",
                table: "Rewards",
                newName: "IX_Rewards_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_ride_id",
                table: "Rewards",
                newName: "IX_Rewards_RideId");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "Reviews",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "id_reservation",
                table: "Reviews",
                newName: "RideId");

            migrationBuilder.RenameColumn(
                name: "commentaire",
                table: "Reviews",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "id_evaluation",
                table: "Reviews",
                newName: "ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_id_reservation",
                table: "Reviews",
                newName: "IX_Reviews_RideId");

            migrationBuilder.RenameColumn(
                name: "montant",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "methode",
                table: "Payments",
                newName: "Method");

            migrationBuilder.RenameColumn(
                name: "id_reservation",
                table: "Payments",
                newName: "RideId");

            migrationBuilder.RenameColumn(
                name: "date_paiement",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "id_paiement",
                table: "Payments",
                newName: "PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_id_reservation",
                table: "Payments",
                newName: "IX_Payments_RideId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Rewards",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "Reviews",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Rides_RideId",
                table: "Payments",
                column: "RideId",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Rides_RideId",
                table: "Reviews",
                column: "RideId",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_AspNetUsers_UserId",
                table: "Rewards",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Rides_RideId",
                table: "Rewards",
                column: "RideId",
                principalTable: "Rides",
                principalColumn: "ride_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
