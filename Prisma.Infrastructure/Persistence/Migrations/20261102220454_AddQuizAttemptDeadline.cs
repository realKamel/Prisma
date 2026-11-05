using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAttemptDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deadline",
                table: "QuizAttempt",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql("""
                UPDATE "QuizAttempt" a
                SET "Deadline" = a."StartedAt" + q."TimeInMinutes"
                FROM "Quiz" q
                WHERE q."Id" = a."QuizId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempt_Status_Deadline",
                table: "QuizAttempt",
                columns: new[] { "Status", "Deadline" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempt_Status_Deadline",
                table: "QuizAttempt");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "QuizAttempt");
        }
    }
}
