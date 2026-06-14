using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addIsCompletedProptoEnrollmententity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "RedeemCode",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "Enrollment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Enrollment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Enrollment");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "RedeemCode",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
