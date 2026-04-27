using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabResource.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnedAt",
                table: "BorrowingRecords",
                newName: "ActualReturnedAt");

            migrationBuilder.RenameColumn(
                name: "BorrowedAt",
                table: "BorrowingRecords",
                newName: "RequestedStartDate");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTeacherId",
                table: "LabAssets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "LabAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualBorrowedAt",
                table: "BorrowingRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedEndDate",
                table: "BorrowingRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BorrowingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LabAssets_AssignedTeacherId",
                table: "LabAssets",
                column: "AssignedTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabAssets_Users_AssignedTeacherId",
                table: "LabAssets",
                column: "AssignedTeacherId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabAssets_Users_AssignedTeacherId",
                table: "LabAssets");

            migrationBuilder.DropIndex(
                name: "IX_LabAssets_AssignedTeacherId",
                table: "LabAssets");

            migrationBuilder.DropColumn(
                name: "AssignedTeacherId",
                table: "LabAssets");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "LabAssets");

            migrationBuilder.DropColumn(
                name: "ActualBorrowedAt",
                table: "BorrowingRecords");

            migrationBuilder.DropColumn(
                name: "RequestedEndDate",
                table: "BorrowingRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BorrowingRecords");

            migrationBuilder.RenameColumn(
                name: "RequestedStartDate",
                table: "BorrowingRecords",
                newName: "BorrowedAt");

            migrationBuilder.RenameColumn(
                name: "ActualReturnedAt",
                table: "BorrowingRecords",
                newName: "ReturnedAt");
        }
    }
}
