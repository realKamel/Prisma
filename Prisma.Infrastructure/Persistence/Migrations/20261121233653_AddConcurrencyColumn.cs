using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "LessonMaterial");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "VideoLibrary",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ObjectStorage",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "LessonMaterial",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Lesson",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Enrollment",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ObjectStorage");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "LessonMaterial");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Enrollment");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "LessonMaterial",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
