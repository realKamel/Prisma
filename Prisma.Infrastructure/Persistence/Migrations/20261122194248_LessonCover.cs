using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LessonCover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageFileObjectId",
                table: "Lesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_ImageFileObjectId",
                table: "Lesson",
                column: "ImageFileObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_FileObject_ImageFileObjectId",
                table: "Lesson",
                column: "ImageFileObjectId",
                principalTable: "FileObject",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_FileObject_ImageFileObjectId",
                table: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_Lesson_ImageFileObjectId",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "ImageFileObjectId",
                table: "Lesson");
        }
    }
}
