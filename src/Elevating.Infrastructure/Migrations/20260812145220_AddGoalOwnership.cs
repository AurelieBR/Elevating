using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elevating.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Goals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_OwnerId",
                table: "Goals",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_AspNetUsers_OwnerId",
                table: "Goals",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_AspNetUsers_OwnerId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_OwnerId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Goals");
        }
    }
}
