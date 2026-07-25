using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "Section",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptSummary",
                table: "Section",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "TranscriptSummary",
                table: "Section");
        }
    }
}
