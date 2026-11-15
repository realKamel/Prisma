using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacherProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Color",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Facebook",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Instagram",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Telegram",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Tiktok",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Website",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Whatsapp",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Links_Youtube",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileImageFileId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Visibility_ShowContactLinks",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Visibility_ShowLessonsCount",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Visibility_ShowPopularLessons",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Visibility_ShowStudentsCount",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Visibility_ShowYearsOfExperience",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfileImageFileId",
                table: "Users",
                column: "ProfileImageFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_FileObject_ProfileImageFileId",
                table: "Users",
                column: "ProfileImageFileId",
                principalTable: "FileObject",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_FileObject_ProfileImageFileId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProfileImageFileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Facebook",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Instagram",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Telegram",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Tiktok",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Website",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Whatsapp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Links_Youtube",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageFileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Visibility_ShowContactLinks",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Visibility_ShowLessonsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Visibility_ShowPopularLessons",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Visibility_ShowStudentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Visibility_ShowYearsOfExperience",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Users");
        }
    }
}
