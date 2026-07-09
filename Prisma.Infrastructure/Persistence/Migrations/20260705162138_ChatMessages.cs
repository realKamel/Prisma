using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prisma.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSession_UserId_AgentName",
                table: "ChatSession");

            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "ChatSession");

            migrationBuilder.RenameColumn(
                name: "ThreadState",
                table: "ChatSession",
                newName: "SerializedSessionJson");

            migrationBuilder.RenameColumn(
                name: "LastActivityUtc",
                table: "ChatSession",
                newName: "UpdatedAtUtc");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "ChatSession",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId",
                table: "ChatSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSession_UserId",
                table: "ChatSession");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ChatSession");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "ChatSession",
                newName: "LastActivityUtc");

            migrationBuilder.RenameColumn(
                name: "SerializedSessionJson",
                table: "ChatSession",
                newName: "ThreadState");

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "ChatSession",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId_AgentName",
                table: "ChatSession",
                columns: new[] { "UserId", "AgentName" });
        }
    }
}
