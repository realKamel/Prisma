using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixingSoftDeleteIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearLesson_AcademicYear_AcademicYearsId",
                table: "AcademicYearLesson");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearLesson_Lesson_LessonsId",
                table: "AcademicYearLesson");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_AcademicYear_AcademicYearsId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeachersId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_LessonId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_QuestionLessonQuiz_LessonQuizId_QuestionId",
                table: "QuestionLessonQuiz");

            migrationBuilder.DropIndex(
                name: "IX_AttemptAnswer_QuizAttemptId_QuestionId",
                table: "AttemptAnswer");

            migrationBuilder.DropIndex(
                name: "IX_Assignment_LessonId",
                table: "Assignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYearTeacher",
                table: "AcademicYearTeacher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYearLesson",
                table: "AcademicYearLesson");

            migrationBuilder.RenameColumn(
                name: "TeachersId",
                table: "AcademicYearTeacher",
                newName: "TeacherId1");

            migrationBuilder.RenameColumn(
                name: "AcademicYearsId",
                table: "AcademicYearTeacher",
                newName: "TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYearTeacher_TeachersId",
                table: "AcademicYearTeacher",
                newName: "IX_AcademicYearTeacher_TeacherId1");

            migrationBuilder.RenameColumn(
                name: "LessonsId",
                table: "AcademicYearLesson",
                newName: "LessonId");

            migrationBuilder.RenameColumn(
                name: "AcademicYearsId",
                table: "AcademicYearLesson",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYearLesson_LessonsId",
                table: "AcademicYearLesson",
                newName: "IX_AcademicYearLesson_LessonId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AcademicYearTeacher",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "AcademicYearTeacher",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AcademicYearTeacher",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AcademicYearTeacher",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "AcademicYearTeacher",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "AcademicYearTeacher",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AcademicYearTeacher",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AcademicYearTeacher",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "AcademicYearTeacher",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AcademicYearLesson",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AcademicYearLesson",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AcademicYearLesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "AcademicYearLesson",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "AcademicYearLesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AcademicYearLesson",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AcademicYearLesson",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "AcademicYearLesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYearTeacher",
                table: "AcademicYearTeacher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYearLesson",
                table: "AcademicYearLesson",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_LessonId",
                table: "Quiz",
                column: "LessonId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLessonQuiz_LessonQuizId_QuestionId",
                table: "QuestionLessonQuiz",
                columns: new[] { "LessonQuizId", "QuestionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswer_QuizAttemptId_QuestionId",
                table: "AttemptAnswer",
                columns: new[] { "QuizAttemptId", "QuestionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_LessonId",
                table: "Assignment",
                column: "LessonId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearTeacher_AcademicYearId_TeacherId",
                table: "AcademicYearTeacher",
                columns: new[] { "AcademicYearId", "TeacherId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearLesson_AcademicYearId_LessonId",
                table: "AcademicYearLesson",
                columns: new[] { "AcademicYearId", "LessonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearLesson_AcademicYear_AcademicYearId",
                table: "AcademicYearLesson",
                column: "AcademicYearId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearLesson_Lesson_LessonId",
                table: "AcademicYearLesson",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_AcademicYear_AcademicYearId",
                table: "AcademicYearTeacher",
                column: "AcademicYearId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId1",
                table: "AcademicYearTeacher",
                column: "TeacherId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearLesson_AcademicYear_AcademicYearId",
                table: "AcademicYearLesson");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearLesson_Lesson_LessonId",
                table: "AcademicYearLesson");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_AcademicYear_AcademicYearId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeacherId1",
                table: "AcademicYearTeacher");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_LessonId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_QuestionLessonQuiz_LessonQuizId_QuestionId",
                table: "QuestionLessonQuiz");

            migrationBuilder.DropIndex(
                name: "IX_AttemptAnswer_QuizAttemptId_QuestionId",
                table: "AttemptAnswer");

            migrationBuilder.DropIndex(
                name: "IX_Assignment_LessonId",
                table: "Assignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYearTeacher",
                table: "AcademicYearTeacher");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYearTeacher_AcademicYearId_TeacherId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYearLesson",
                table: "AcademicYearLesson");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYearLesson_AcademicYearId_LessonId",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AcademicYearTeacher");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AcademicYearLesson");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AcademicYearLesson");

            migrationBuilder.RenameColumn(
                name: "TeacherId1",
                table: "AcademicYearTeacher",
                newName: "TeachersId");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "AcademicYearTeacher",
                newName: "AcademicYearsId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYearTeacher_TeacherId1",
                table: "AcademicYearTeacher",
                newName: "IX_AcademicYearTeacher_TeachersId");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "AcademicYearLesson",
                newName: "LessonsId");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "AcademicYearLesson",
                newName: "AcademicYearsId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYearLesson_LessonId",
                table: "AcademicYearLesson",
                newName: "IX_AcademicYearLesson_LessonsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYearTeacher",
                table: "AcademicYearTeacher",
                columns: new[] { "AcademicYearsId", "TeachersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYearLesson",
                table: "AcademicYearLesson",
                columns: new[] { "AcademicYearsId", "LessonsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_LessonId",
                table: "Quiz",
                column: "LessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLessonQuiz_LessonQuizId_QuestionId",
                table: "QuestionLessonQuiz",
                columns: new[] { "LessonQuizId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswer_QuizAttemptId_QuestionId",
                table: "AttemptAnswer",
                columns: new[] { "QuizAttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_LessonId",
                table: "Assignment",
                column: "LessonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearLesson_AcademicYear_AcademicYearsId",
                table: "AcademicYearLesson",
                column: "AcademicYearsId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearLesson_Lesson_LessonsId",
                table: "AcademicYearLesson",
                column: "LessonsId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_AcademicYear_AcademicYearsId",
                table: "AcademicYearTeacher",
                column: "AcademicYearsId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYearTeacher_Users_TeachersId",
                table: "AcademicYearTeacher",
                column: "TeachersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
