using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "ProvisioningStatus",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "Video");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderLibraryId",
                table: "VideoLibrary",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VideoLibrary",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Video",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary",
                column: "ProviderLibraryId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProviderLibraryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Video_ProviderVideoLibraryId_ProviderVideoId",
                table: "Video",
                columns: new[] { "ProviderVideoLibraryId", "ProviderVideoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_FileObjectId",
                table: "Assignment",
                column: "FileObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignment_FileObject_FileObjectId",
                table: "Assignment",
                column: "FileObjectId",
                principalTable: "FileObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignment_FileObject_FileObjectId",
                table: "Assignment");

            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary");

            migrationBuilder.DropIndex(
                name: "IX_Video_ProviderVideoLibraryId_ProviderVideoId",
                table: "Video");

            migrationBuilder.DropIndex(
                name: "IX_Assignment_FileObjectId",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Video");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderLibraryId",
                table: "VideoLibrary",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisioningStatus",
                table: "VideoLibrary",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "VideoLibrary",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingStatus",
                table: "Video",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary",
                column: "ProviderLibraryId",
                filter: "\"IsDeleted\" = false");
        }
    }
}
