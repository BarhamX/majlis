using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedDailyMajlisRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "OptionKey", table: "ChallengeOptions", type: "text", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PublishedRevisionId",
                table: "DailyMajlis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduledRevisionId",
                table: "DailyMajlis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevisionId",
                table: "Challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChallengeOptionTranslations",
                columns: table => new
                {
                    OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeOptionTranslations", x => new { x.OptionId, x.Locale });
                    table.ForeignKey(
                        name: "FK_ChallengeOptionTranslations_ChallengeOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "ChallengeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyMajlisRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyMajlisId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    TopicCode = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    CardType = table.Column<string>(type: "text", nullable: false),
                    SourceNotes = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupersedesRevisionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMajlisRevisions", x => x.Id);
                    table.CheckConstraint("CK_DailyMajlisRevisions_SourceNotes", "length(btrim(\"SourceNotes\")) > 0");
                    table.ForeignKey(
                        name: "FK_DailyMajlisRevisions_DailyMajlisRevisions_SupersedesRevisio~",
                        column: x => x.SupersedesRevisionId,
                        principalTable: "DailyMajlisRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyMajlisRevisions_DailyMajlis_DailyMajlisId",
                        column: x => x.DailyMajlisId,
                        principalTable: "DailyMajlis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyMajlisTranslations",
                columns: table => new
                {
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false),
                    DiscussionPrompt = table.Column<string>(type: "text", nullable: false),
                    CardTitle = table.Column<string>(type: "text", nullable: true),
                    CardText = table.Column<string>(type: "text", nullable: false),
                    CardMeaning = table.Column<string>(type: "text", nullable: true),
                    CardContext = table.Column<string>(type: "text", nullable: true),
                    Transliteration = table.Column<string>(type: "text", nullable: true),
                    PublicAttribution = table.Column<string>(type: "text", nullable: true),
                    CorrectionNote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMajlisTranslations", x => new { x.RevisionId, x.Locale });
                    table.ForeignKey(
                        name: "FK_DailyMajlisTranslations_DailyMajlisRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "DailyMajlisRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionDialects",
                columns: table => new
                {
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DialectCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionDialects", x => new { x.RevisionId, x.DialectCode });
                    table.ForeignKey(
                        name: "FK_RevisionDialects_DailyMajlisRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "DailyMajlisRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionRegions",
                columns: table => new
                {
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionRegions", x => new { x.RevisionId, x.RegionCode });
                    table.ForeignKey(
                        name: "FK_RevisionRegions_DailyMajlisRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "DailyMajlisRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlis_PublishedRevisionId",
                table: "DailyMajlis",
                column: "PublishedRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlis_ScheduledRevisionId",
                table: "DailyMajlis",
                column: "ScheduledRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_RevisionId",
                table: "Challenges",
                column: "RevisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_OptionKey_SortOrder",
                table: "ChallengeOptions",
                columns: new[] { "OptionKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber",
                table: "DailyMajlisRevisions",
                columns: new[] { "DailyMajlisId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlisRevisions_SupersedesRevisionId",
                table: "DailyMajlisRevisions",
                column: "SupersedesRevisionId");

            migrationBuilder.Sql("""
                INSERT INTO "DailyMajlisRevisions"
                    ("Id", "DailyMajlisId", "RevisionNumber", "TopicCode", "Difficulty", "CardType",
                     "SourceNotes", "CreatedByUserId", "CreatedAt")
                SELECT md5(d."Id"::text)::uuid, d."Id", 1, c."Topic", c."Difficulty", 'proverb',
                       COALESCE(NULLIF(btrim(c."SourceNotes"), ''),
                           'Legacy content migrated without verified editorial source notes.'),
                       NULL, d."CreatedAt"
                FROM "DailyMajlis" d
                JOIN "Challenges" c ON c."Id" = d."ChallengeId"
                WHERE NOT EXISTS (
                    SELECT 1 FROM "DailyMajlisRevisions" r WHERE r."DailyMajlisId" = d."Id");

                INSERT INTO "DailyMajlisTranslations"
                    ("RevisionId", "Locale", "Title", "QuestionText", "Explanation", "DiscussionPrompt", "CardText")
                SELECT r."Id", 'en', d."Title", c."QuestionText", c."Explanation", d."DiscussionQuestion", c."Explanation"
                FROM "DailyMajlisRevisions" r
                JOIN "DailyMajlis" d ON d."Id" = r."DailyMajlisId"
                JOIN "Challenges" c ON c."Id" = d."ChallengeId"
                ON CONFLICT ("RevisionId", "Locale") DO NOTHING;

                INSERT INTO "ChallengeOptionTranslations" ("OptionId", "Locale", "Text")
                SELECT o."Id", 'en', o."Text" FROM "ChallengeOptions" o
                ON CONFLICT ("OptionId", "Locale") DO NOTHING;

                UPDATE "ChallengeOptions" SET "OptionKey" = chr(96 + "SortOrder") WHERE "OptionKey" IS NULL;
                UPDATE "Challenges" c
                SET "RevisionId" = md5(d."Id"::text)::uuid
                FROM "DailyMajlis" d
                WHERE d."ChallengeId" = c."Id" AND c."RevisionId" IS NULL;
                UPDATE "DailyMajlis"
                SET "PublishedRevisionId" = NULL, "ScheduledRevisionId" = NULL,
                    "Status" = CASE WHEN "Status" IN ('published', 'scheduled') THEN 'unpublished' ELSE "Status" END
                WHERE "Status" IN ('published', 'scheduled');
                """);

            migrationBuilder.AlterColumn<Guid>(name: "RevisionId", table: "Challenges", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "OptionKey", table: "ChallengeOptions", type: "text", nullable: false,
                oldClrType: typeof(string), oldType: "text", oldNullable: true);
            migrationBuilder.DropForeignKey(name: "FK_DailyMajlis_Challenges_ChallengeId", table: "DailyMajlis");
            migrationBuilder.DropIndex(name: "IX_DailyMajlis_ChallengeId", table: "DailyMajlis");
            migrationBuilder.DropColumn(name: "ChallengeId", table: "DailyMajlis");
            migrationBuilder.DropColumn(name: "DiscussionQuestion", table: "DailyMajlis");
            migrationBuilder.DropColumn(name: "Title", table: "DailyMajlis");
            migrationBuilder.DropColumn(name: "Topic", table: "DailyMajlis");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Challenges");
            migrationBuilder.DropColumn(name: "Difficulty", table: "Challenges");
            migrationBuilder.DropColumn(name: "Explanation", table: "Challenges");
            migrationBuilder.DropColumn(name: "QuestionText", table: "Challenges");
            migrationBuilder.DropColumn(name: "Region", table: "Challenges");
            migrationBuilder.DropColumn(name: "ReviewStatus", table: "Challenges");
            migrationBuilder.DropColumn(name: "SourceNotes", table: "Challenges");
            migrationBuilder.DropColumn(name: "Topic", table: "Challenges");
            migrationBuilder.DropColumn(name: "Text", table: "ChallengeOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_DailyMajlisRevisions_RevisionId",
                table: "Challenges",
                column: "RevisionId",
                principalTable: "DailyMajlisRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyMajlis_DailyMajlisRevisions_PublishedRevisionId",
                table: "DailyMajlis",
                column: "PublishedRevisionId",
                principalTable: "DailyMajlisRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyMajlis_DailyMajlisRevisions_ScheduledRevisionId",
                table: "DailyMajlis",
                column: "ScheduledRevisionId",
                principalTable: "DailyMajlisRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_DailyMajlisRevisions_RevisionId",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyMajlis_DailyMajlisRevisions_PublishedRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyMajlis_DailyMajlisRevisions_ScheduledRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropTable(
                name: "ChallengeOptionTranslations");

            migrationBuilder.DropTable(
                name: "DailyMajlisTranslations");

            migrationBuilder.DropTable(
                name: "RevisionDialects");

            migrationBuilder.DropTable(
                name: "RevisionRegions");

            migrationBuilder.DropTable(
                name: "DailyMajlisRevisions");

            migrationBuilder.DropIndex(
                name: "IX_DailyMajlis_PublishedRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropIndex(
                name: "IX_DailyMajlis_ScheduledRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_RevisionId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeOptions_OptionKey_SortOrder",
                table: "ChallengeOptions");

            migrationBuilder.DropColumn(
                name: "PublishedRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropColumn(
                name: "ScheduledRevisionId",
                table: "DailyMajlis");

            migrationBuilder.DropColumn(
                name: "RevisionId",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "OptionKey",
                table: "ChallengeOptions",
                newName: "Text");

            migrationBuilder.AddColumn<Guid>(
                name: "ChallengeId",
                table: "DailyMajlis",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "DiscussionQuestion",
                table: "DailyMajlis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "DailyMajlis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "DailyMajlis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Challenges",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "Challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "Challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Challenges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "Challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceNotes",
                table: "Challenges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMajlis_ChallengeId",
                table: "DailyMajlis",
                column: "ChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyMajlis_Challenges_ChallengeId",
                table: "DailyMajlis",
                column: "ChallengeId",
                principalTable: "Challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
