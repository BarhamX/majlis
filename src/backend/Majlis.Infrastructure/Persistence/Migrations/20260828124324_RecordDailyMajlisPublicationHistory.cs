using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RecordDailyMajlisPublicationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyMajlisPublications",
                columns: table => new
                {
                    DailyMajlisId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMajlisPublications", x => x.DailyMajlisId);
                    table.ForeignKey(
                        name: "FK_DailyMajlisPublications_DailyMajlis_DailyMajlisId",
                        column: x => x.DailyMajlisId,
                        principalTable: "DailyMajlis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Legacy rows have no exact first-publication timestamp. Current published and
            // explicitly unpublished rows both prove that their date was once eligible;
            // UpdatedAt is the safest available backfill timestamp. DISTINCT ON makes a
            // deterministic choice if legacy data contains more than one unpublished row.
            migrationBuilder.Sql("""
                INSERT INTO "DailyMajlisPublications" ("DailyMajlisId", "PublishDate", "PublishedAt")
                SELECT DISTINCT ON (daily."PublishDate")
                    daily."Id", daily."PublishDate", daily."UpdatedAt"
                FROM "DailyMajlis" AS daily
                WHERE daily."Status" IN ('published', 'unpublished')
                ORDER BY daily."PublishDate",
                    CASE WHEN daily."Status" = 'published' THEN 0 ELSE 1 END,
                    daily."UpdatedAt" ASC,
                    daily."Id" ASC;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlisPublications_PublishDate",
                table: "DailyMajlisPublications",
                column: "PublishDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Daily Majlis publication history is a forward-only boundary; " +
                "restore a compatible backup or apply a reviewed forward recovery migration.");
        }
    }
}
