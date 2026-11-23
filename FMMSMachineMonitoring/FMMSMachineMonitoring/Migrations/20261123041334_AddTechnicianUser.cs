using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMMSMachineMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Technicians",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_UserId",
                table: "Technicians",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_AspNetUsers_UserId",
                table: "Technicians",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_AspNetUsers_UserId",
                table: "Technicians");

            migrationBuilder.DropIndex(
                name: "IX_Technicians_UserId",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Technicians");
        }
    }
}
