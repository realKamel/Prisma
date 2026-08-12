using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssistantTeacherRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_Assistant_TeacherId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_Assistant_TeacherId",
                table: "Users",
                column: "Assistant_TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_Assistant_TeacherId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_Assistant_TeacherId",
                table: "Users",
                column: "Assistant_TeacherId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
