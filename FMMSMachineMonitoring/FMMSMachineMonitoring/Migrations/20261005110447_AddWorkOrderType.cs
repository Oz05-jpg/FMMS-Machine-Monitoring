using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMMSMachineMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "WorkOrders");
        }
    }
}
