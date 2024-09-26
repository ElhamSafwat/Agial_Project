using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final_project_Api.Migrations
{
    /// <inheritdoc />
    public partial class studentStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "students");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "students");
        }
    }
}
