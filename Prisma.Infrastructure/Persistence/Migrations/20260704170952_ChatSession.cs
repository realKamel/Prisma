using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", maxLength: 450, nullable: true),
                    AgentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ThreadState = table.Column<string>(type: "jsonb", nullable: false),
                    LastActivityUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSession", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId_AgentName",
                table: "ChatSession",
                columns: new[] { "UserId", "AgentName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatSession");
        }
    }
}
