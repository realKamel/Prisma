using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoLibrary_Users_TeacherId",
                table: "VideoLibrary");

            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_TeacherId",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "VideoLibraryId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "EncodingTier",
                table: "VideoLibrary",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Section",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "AuditLog",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_TeacherId",
                table: "VideoLibrary",
                column: "TeacherId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoLibrary_TeacherId",
                table: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "EncodingTier",
                table: "VideoLibrary");

            migrationBuilder.AddColumn<int>(
                name: "VideoLibraryId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Section",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AuditLog",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_TeacherId",
                table: "VideoLibrary",
                column: "TeacherId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoLibrary_Users_TeacherId",
                table: "VideoLibrary",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
