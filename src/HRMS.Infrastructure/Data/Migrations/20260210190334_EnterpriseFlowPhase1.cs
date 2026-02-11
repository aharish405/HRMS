using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseFlowPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemGenerated",
                table: "Salaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OfferLetterId",
                table: "Salaries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceToken",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedOn",
                table: "OfferLetters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAddresses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_OfferLetterId",
                table: "Salaries",
                column: "OfferLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAddresses_EmployeeId_IsPrimary",
                table: "EmployeeAddresses",
                columns: new[] { "EmployeeId", "IsPrimary" });

            migrationBuilder.AddForeignKey(
                name: "FK_Salaries_OfferLetters_OfferLetterId",
                table: "Salaries",
                column: "OfferLetterId",
                principalTable: "OfferLetters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Salaries_OfferLetters_OfferLetterId",
                table: "Salaries");

            migrationBuilder.DropTable(
                name: "EmployeeAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Salaries_OfferLetterId",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "IsSystemGenerated",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "OfferLetterId",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "AcceptanceToken",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "AcceptedOn",
                table: "OfferLetters");
        }
    }
}
