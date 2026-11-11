using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EntitesUpdatedAndDropUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "DownloadUrl",
                table: "LessonMaterial");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "LessonMaterial");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "LessonMaterial");

            migrationBuilder.AddColumn<Guid>(
                name: "FileObjectId",
                table: "LessonMaterial",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary",
                column: "ProviderLibraryId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LessonMaterial_FileObjectId",
                table: "LessonMaterial",
                column: "FileObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonMaterial_FileObject_FileObjectId",
                table: "LessonMaterial",
                column: "FileObjectId",
                principalTable: "FileObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonMaterial_FileObject_FileObjectId",
                table: "LessonMaterial");

            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary");

            migrationBuilder.DropIndex(
                name: "IX_LessonMaterial_FileObjectId",
                table: "LessonMaterial");

            migrationBuilder.DropColumn(
                name: "FileObjectId",
                table: "LessonMaterial");

            migrationBuilder.AddColumn<string>(
                name: "DownloadUrl",
                table: "LessonMaterial",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "LessonMaterial",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LessonMaterial",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary",
                column: "ProviderLibraryId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
