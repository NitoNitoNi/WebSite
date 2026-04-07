using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDataDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Employees_EmployeeId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Resources_ResourceId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_EmployeeId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ResourceId",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EmployeeId",
                table: "Accounts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ResourceId",
                table: "Accounts",
                column: "ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Employees_EmployeeId",
                table: "Accounts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Resources_ResourceId",
                table: "Accounts",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "ResourcesId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
