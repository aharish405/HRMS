using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeKYCFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AcceptanceToken",
                table: "OfferLetters",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AadharNumber",
                table: "Employees",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Employees",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "Employees",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_AcceptanceToken",
                table: "OfferLetters",
                column: "AcceptanceToken");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_AadharNumber",
                table: "Employees",
                column: "AadharNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PanNumber",
                table: "Employees",
                column: "PanNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OfferLetters_AcceptanceToken",
                table: "OfferLetters");

            migrationBuilder.DropIndex(
                name: "IX_Employees_AadharNumber",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PanNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AadharNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "AcceptanceToken",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
