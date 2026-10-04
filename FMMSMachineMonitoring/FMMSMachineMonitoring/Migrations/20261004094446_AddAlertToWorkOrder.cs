using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMMSMachineMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertToWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertId",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AlertId",
                table: "WorkOrders",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Alerts_AlertId",
                table: "WorkOrders",
                column: "AlertId",
                principalTable: "Alerts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Alerts_AlertId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AlertId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AlertId",
                table: "WorkOrders");
        }
    }
}
