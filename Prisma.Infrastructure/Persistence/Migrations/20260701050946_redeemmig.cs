using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class redeemmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId1",
                table: "AcademicYearTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_RedeemCode_RedeemCodeId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_RedeemCodeId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYearTeacher_TeacherId1",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "RedeemedAt",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "RedeemedByStudentId",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "AcademicYearTeacher");

            migrationBuilder.RenameColumn(
                name: "UsedCount",
                table: "RedeemCode",
                newName: "TotalCodes");

            migrationBuilder.RenameColumn(
                name: "MaxUses",
                table: "RedeemCode",
                newName: "AcademicYearId");

            migrationBuilder.RenameColumn(
                name: "RedeemCodeId",
                table: "Enrollment",
                newName: "GeneratedCodeId");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "RedeemCode",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByTeacherId",
                table: "RedeemCode",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "RedeemCode",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql("""
    ALTER TABLE "AcademicYearTeacher" 
        DROP CONSTRAINT IF EXISTS "FK_AcademicYearTeacher_Users_TeacherId";
    ALTER TABLE "AcademicYearTeacher" 
        DROP COLUMN "TeacherId";
    ALTER TABLE "AcademicYearTeacher" 
        ADD COLUMN "TeacherId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    """);

            migrationBuilder.CreateTable(
                name: "GeneratedCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RedeemedByStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedCode_RedeemCode_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RedeemCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedCode_Users_RedeemedByStudentId",
                        column: x => x.RedeemedByStudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedeemCode_AcademicYearId",
                table: "RedeemCode",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_RedeemCode_CreatedByTeacherId",
                table: "RedeemCode",
                column: "CreatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_GeneratedCodeId",
                table: "Enrollment",
                column: "GeneratedCodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearTeacher_TeacherId",
                table: "AcademicYearTeacher",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedCode_BatchId_Code",
                table: "GeneratedCode",
                columns: new[] { "BatchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedCode_RedeemedByStudentId",
                table: "GeneratedCode",
                column: "RedeemedByStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId",
                table: "AcademicYearTeacher",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_GeneratedCode_GeneratedCodeId",
                table: "Enrollment",
                column: "GeneratedCodeId",
                principalTable: "GeneratedCode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_AcademicYear_AcademicYearId",
                table: "RedeemCode",
                column: "AcademicYearId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_Users_CreatedByTeacherId",
                table: "RedeemCode",
                column: "CreatedByTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_GeneratedCode_GeneratedCodeId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_AcademicYear_AcademicYearId",
                table: "RedeemCode");

            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode");

            migrationBuilder.DropForeignKey(
                name: "FK_RedeemCode_Users_CreatedByTeacherId",
                table: "RedeemCode");

            migrationBuilder.DropTable(
                name: "GeneratedCode");

            migrationBuilder.DropIndex(
                name: "IX_RedeemCode_AcademicYearId",
                table: "RedeemCode");

            migrationBuilder.DropIndex(
                name: "IX_RedeemCode_CreatedByTeacherId",
                table: "RedeemCode");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_GeneratedCodeId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYearTeacher_TeacherId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "CreatedByTeacherId",
                table: "RedeemCode");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "RedeemCode");

            migrationBuilder.RenameColumn(
                name: "TotalCodes",
                table: "RedeemCode",
                newName: "UsedCount");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "RedeemCode",
                newName: "MaxUses");

            migrationBuilder.RenameColumn(
                name: "GeneratedCodeId",
                table: "Enrollment",
                newName: "RedeemCodeId");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "RedeemCode",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "RedeemCode",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "RedeemCode",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RedeemedAt",
                table: "RedeemCode",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RedeemedByStudentId",
                table: "RedeemCode",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "AcademicYearTeacher",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId1",
                table: "AcademicYearTeacher",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_RedeemCodeId",
                table: "Enrollment",
                column: "RedeemCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearTeacher_TeacherId1",
                table: "AcademicYearTeacher",
                column: "TeacherId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId1",
                table: "AcademicYearTeacher",
                column: "TeacherId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_RedeemCode_RedeemCodeId",
                table: "Enrollment",
                column: "RedeemCodeId",
                principalTable: "RedeemCode",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RedeemCode_Lesson_LessonId",
                table: "RedeemCode",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id");
        }
    }
}
