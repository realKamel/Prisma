using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIdInQuizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Quiz",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_TeacherId",
                table: "Quiz",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Users_TeacherId",
                table: "Quiz",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Users_TeacherId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_TeacherId",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Quiz");
        }
    }
}
