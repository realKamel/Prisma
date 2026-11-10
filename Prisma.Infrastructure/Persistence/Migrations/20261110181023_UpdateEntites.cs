using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Assignment");

            migrationBuilder.AddColumn<decimal>(
                name: "AssignmentScore_AchievedPoints",
                table: "AssignmentSubmission",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AssignmentScore_MaxPoints",
                table: "AssignmentSubmission",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AssignmentSubmission",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DueDate",
                table: "Assignment",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "Expiry_Seconds",
                table: "Assignment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "FileObjectId",
                table: "Assignment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMarks",
                table: "Assignment",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentScore_AchievedPoints",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "AssignmentScore_MaxPoints",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "AssignmentSubmission");

            migrationBuilder.DropColumn(
                name: "Expiry_Seconds",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "FileObjectId",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "TotalMarks",
                table: "Assignment");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DueDate",
                table: "Assignment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Assignment",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
