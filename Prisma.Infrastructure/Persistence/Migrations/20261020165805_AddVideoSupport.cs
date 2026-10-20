using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoLibraryId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "Section",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VideoLibrary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderLibraryId = table.Column<long>(type: "bigint", nullable: false),
                    TrafficUsage = table.Column<long>(type: "bigint", nullable: false),
                    StorageUsage = table.Column<long>(type: "bigint", nullable: false),
                    LibraryApiKey = table.Column<string>(type: "text", nullable: true),
                    ReadOnlyApiKey = table.Column<string>(type: "text", nullable: true),
                    WebhookUrl = table.Column<string>(type: "text", nullable: true),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvisioningStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_VideoLibrary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoLibrary_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Video",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoLibraryId = table.Column<int>(type: "integer", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    ProviderVideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderVideoLibraryId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    StorageSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    TimeDuration_Seconds = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Video", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Video_Section_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Section",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Video_VideoLibrary_VideoLibraryId",
                        column: x => x.VideoLibraryId,
                        principalTable: "VideoLibrary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Video_ProviderVideoId",
                table: "Video",
                column: "ProviderVideoId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Video_SectionId",
                table: "Video",
                column: "SectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Video_VideoLibraryId",
                table: "Video",
                column: "VideoLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_ProviderLibraryId",
                table: "VideoLibrary",
                column: "ProviderLibraryId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibrary_TeacherId",
                table: "VideoLibrary",
                column: "TeacherId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Video");

            migrationBuilder.DropTable(
                name: "VideoLibrary");

            migrationBuilder.DropColumn(
                name: "VideoLibraryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Section");
        }
    }
}
