using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixExtractionJobV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Questions",
                table: "ExtractionJobs");

            migrationBuilder.AddColumn<string>(
                name: "QuestionsJson",
                table: "ExtractionJobs",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionsJson",
                table: "ExtractionJobs");

            migrationBuilder.AddColumn<string>(
                name: "Questions",
                table: "ExtractionJobs",
                type: "jsonb",
                nullable: true);
        }
    }
}
