using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Lesson",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "Lesson",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonTranscriptChunk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonTranscriptChunk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonTranscriptChunk_Lesson_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lesson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonTranscriptChunk_Embedding",
                table: "LessonTranscriptChunk",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 64)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "IX_LessonTranscriptChunk_LessonId",
                table: "LessonTranscriptChunk",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "Lesson");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
