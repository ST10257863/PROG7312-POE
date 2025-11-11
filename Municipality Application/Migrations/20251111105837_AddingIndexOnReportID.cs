using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Municipality_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddingIndexOnReportID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reports_Id",
                table: "Reports",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_Id",
                table: "Reports");
        }
    }
}
