using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestion_pharma.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoCodeAndFidelityPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppliedPromoCodeId",
                table: "Commandes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PointsEarned",
                table: "Commandes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PromoCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PointsValue = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoCodes_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_AppliedPromoCodeId",
                table: "Commandes",
                column: "AppliedPromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_ClientId",
                table: "PromoCodes",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_PromoCodes_AppliedPromoCodeId",
                table: "Commandes",
                column: "AppliedPromoCodeId",
                principalTable: "PromoCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_PromoCodes_AppliedPromoCodeId",
                table: "Commandes");

            migrationBuilder.DropTable(
                name: "PromoCodes");

            migrationBuilder.DropIndex(
                name: "IX_Commandes_AppliedPromoCodeId",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "AppliedPromoCodeId",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "Commandes");
        }
    }
}
