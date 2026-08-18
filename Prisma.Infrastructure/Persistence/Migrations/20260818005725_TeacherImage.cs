using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TeacherImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill existing NULLs with distinct values before enforcing NOT NULL
            migrationBuilder.Sql(@"
                UPDATE ""Lesson""
                SET ""PublicId"" = gen_random_uuid()
                WHERE ""PublicId"" IS NULL;
            ");

                    migrationBuilder.Sql(@"
                UPDATE ""AcademicYear""
                SET ""PublicId"" = gen_random_uuid()
                WHERE ""PublicId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Lesson",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "AcademicYear",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Lesson",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "AcademicYear",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
