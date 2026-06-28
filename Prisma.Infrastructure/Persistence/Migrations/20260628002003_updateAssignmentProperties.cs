using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateAssignmentProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AssignmentSubmission",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "AssignmentSubmission",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Assignment",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Assignment");
        }
    }
}
