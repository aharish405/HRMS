using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOfferLetterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "GeneratedContent",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "TemplateContent",
                table: "OfferLetters");

            migrationBuilder.RenameColumn(
                name: "Designation",
                table: "OfferLetters",
                newName: "CandidateName");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "OfferLetters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "OfferLetters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalTerms",
                table: "OfferLetters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasicSalary",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CandidateAddress",
                table: "OfferLetters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CandidateEmail",
                table: "OfferLetters",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CandidatePhone",
                table: "OfferLetters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ConveyanceAllowance",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "OfferLetters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DesignationId",
                table: "OfferLetters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HRA",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalAllowance",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowances",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialAllowance",
                table: "OfferLetters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OfferLetters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_CandidateEmail",
                table: "OfferLetters",
                column: "CandidateEmail");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_DepartmentId",
                table: "OfferLetters",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_DesignationId",
                table: "OfferLetters",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_Status",
                table: "OfferLetters",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Departments_DepartmentId",
                table: "OfferLetters",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Designations_DesignationId",
                table: "OfferLetters",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Departments_DepartmentId",
                table: "OfferLetters");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Designations_DesignationId",
                table: "OfferLetters");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters");

            migrationBuilder.DropIndex(
                name: "IX_OfferLetters_CandidateEmail",
                table: "OfferLetters");

            migrationBuilder.DropIndex(
                name: "IX_OfferLetters_DepartmentId",
                table: "OfferLetters");

            migrationBuilder.DropIndex(
                name: "IX_OfferLetters_DesignationId",
                table: "OfferLetters");

            migrationBuilder.DropIndex(
                name: "IX_OfferLetters_Status",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "AdditionalTerms",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "BasicSalary",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "CandidateAddress",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "CandidateEmail",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "CandidatePhone",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "ConveyanceAllowance",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "DesignationId",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "HRA",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "MedicalAllowance",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "OtherAllowances",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "SpecialAllowance",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OfferLetters");

            migrationBuilder.RenameColumn(
                name: "CandidateName",
                table: "OfferLetters",
                newName: "Designation");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "OfferLetters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "OfferLetters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "OfferLetters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "OfferLetters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "OfferLetters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedContent",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateContent",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferLetters_Employees_EmployeeId",
                table: "OfferLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
