using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeductionsToOfferLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters");

            migrationBuilder.AddColumn<decimal>(
                name: "ESI",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetSalary",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductions",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PF",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfessionalTax",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TDS",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "ESI",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "NetSalary",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "OtherDeductions",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "PF",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "ProfessionalTax",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "TDS",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "OfferLetters");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
