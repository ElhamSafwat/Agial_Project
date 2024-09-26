using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final_project_Api.Migrations
{
    /// <inheritdoc />
    public partial class EditSubject1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Subject_ID",
                table: "teacher_Classes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject_ID",
                table: "teacher_Classes");
        }
    }
}
