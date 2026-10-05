using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DomainValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Payment",
                newName: "Money_Currency");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payment",
                newName: "Money_Amount");

            migrationBuilder.AlterColumn<string>(
                name: "Money_Currency",
                table: "Payment",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Money_Amount",
                table: "Payment",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRange_EndDate",
                table: "Enrollment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRange_StartDate",
                table: "Enrollment",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRange_EndDate",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "DateRange_StartDate",
                table: "Enrollment");

            migrationBuilder.RenameColumn(
                name: "Money_Currency",
                table: "Payment",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "Money_Amount",
                table: "Payment",
                newName: "Amount");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payment",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
