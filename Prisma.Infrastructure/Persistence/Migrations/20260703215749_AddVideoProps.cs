using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetId",
                table: "Section",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaybackId",
                table: "Section",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadId",
                table: "Section",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "PlaybackId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "Section");
        }
    }
}
