using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLessonToSupportVo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRange_EndDate",
                table: "Lesson",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRange_StartDate",
                table: "Lesson",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Money_Amount",
                table: "Lesson",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Money_Currency",
                table: "Lesson",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeDuration_Seconds",
                table: "Lesson",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRange_EndDate",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "DateRange_StartDate",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "Money_Amount",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "Money_Currency",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "TimeDuration_Seconds",
                table: "Lesson");
        }
    }
}
