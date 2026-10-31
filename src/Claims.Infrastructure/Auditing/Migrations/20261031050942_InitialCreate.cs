using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Infrastructure.Auditing.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "ClaimAudits",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoverAudits",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoverId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAudits_ClaimId_OccurredAtUtc",
                schema: "audit",
                table: "ClaimAudits",
                columns: new[] { "ClaimId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAudits_EventId",
                schema: "audit",
                table: "ClaimAudits",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoverAudits_CoverId_OccurredAtUtc",
                schema: "audit",
                table: "CoverAudits",
                columns: new[] { "CoverId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CoverAudits_EventId",
                schema: "audit",
                table: "CoverAudits",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimAudits",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CoverAudits",
                schema: "audit");
        }
    }
}
