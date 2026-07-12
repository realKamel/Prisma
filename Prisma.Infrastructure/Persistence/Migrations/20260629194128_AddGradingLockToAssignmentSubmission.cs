using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGradingLockToAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GradingByUserId",
                table: "AssignmentSubmission",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GradingStartedAt",
                table: "AssignmentSubmission",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBeingGraded",
                table: "AssignmentSubmission",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingByUserId",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "GradingStartedAt",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "IsBeingGraded",
                table: "AssignmentSubmission");
        }
    }
}
