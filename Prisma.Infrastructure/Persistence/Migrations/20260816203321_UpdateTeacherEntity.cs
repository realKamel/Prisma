using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacherEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeacherAvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Section",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Lesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "AcademicYear",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Section_PublicId",
                table: "Section",
                column: "PublicId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_PublicId",
                table: "Lesson",
                column: "PublicId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYear_PublicId",
                table: "AcademicYear",
                column: "PublicId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Section_PublicId",
                table: "Section");

            migrationBuilder.DropIndex(
                name: "IX_Lesson_PublicId",
                table: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYear_PublicId",
                table: "AcademicYear");

            migrationBuilder.DropColumn(
                name: "TeacherAvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AcademicYear");
        }
    }
}
