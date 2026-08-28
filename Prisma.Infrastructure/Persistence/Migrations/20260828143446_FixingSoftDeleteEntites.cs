using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixingSoftDeleteEntites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LessonTranscriptChunk",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LessonTranscriptChunk",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "LessonTranscriptChunk",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LessonTranscriptChunk",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LessonTranscriptChunk",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LessonTranscriptChunk",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LessonTranscriptChunk",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LessonTranscriptChunk");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LessonTranscriptChunk");
        }
    }
}
