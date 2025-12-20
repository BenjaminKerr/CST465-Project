using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CST465_project.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBitSizeToVisualization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BitSize",
                table: "Visualizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BitSize",
                table: "Visualizations");
        }
    }
}
