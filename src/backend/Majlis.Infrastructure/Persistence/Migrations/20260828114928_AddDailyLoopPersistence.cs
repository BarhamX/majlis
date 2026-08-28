using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyLoopPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Challenges_Id_RevisionId",
                table: "Challenges",
                columns: new[] { "Id", "RevisionId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ChallengeOptions_Id_ChallengeId",
                table: "ChallengeOptions",
                columns: new[] { "Id", "ChallengeId" });

            migrationBuilder.CreateTable(
                name: "IdempotencyRecords",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "text", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => new { x.UserId, x.Scope, x.IdempotencyKey });
                    table.ForeignKey(
                        name: "FK_IdempotencyRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyMajlisId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionXp = table.Column<int>(type: "integer", nullable: false),
                    CorrectnessXp = table.Column<int>(type: "integer", nullable: false),
                    ResultLocale = table.Column<string>(type: "text", nullable: false),
                    LifetimeXpAfter = table.Column<long>(type: "bigint", nullable: false),
                    CurrentStreakAfter = table.Column<int>(type: "integer", nullable: false),
                    LongestStreakAfter = table.Column<int>(type: "integer", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttempts", x => x.Id);
                    table.CheckConstraint("CK_UserAttempts_ExactXpAndSnapshots", "\"CompletionXp\" = 10 AND \"CorrectnessXp\" IN (0, 5) AND ((\"IsCorrect\" AND \"CorrectnessXp\" = 5) OR (NOT \"IsCorrect\" AND \"CorrectnessXp\" = 0)) AND \"LifetimeXpAfter\" >= 0 AND \"CurrentStreakAfter\" >= 0 AND \"LongestStreakAfter\" >= \"CurrentStreakAfter\"");
                    table.ForeignKey(
                        name: "FK_UserAttempts_ChallengeOptions_SelectedOptionId_ChallengeId",
                        columns: x => new { x.SelectedOptionId, x.ChallengeId },
                        principalTable: "ChallengeOptions",
                        principalColumns: new[] { "Id", "ChallengeId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAttempts_Challenges_ChallengeId_ContentRevisionId",
                        columns: x => new { x.ChallengeId, x.ContentRevisionId },
                        principalTable: "Challenges",
                        principalColumns: new[] { "Id", "RevisionId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAttempts_DailyMajlis_DailyMajlisId",
                        column: x => x.DailyMajlisId,
                        principalTable: "DailyMajlis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgress",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LifetimeXp = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastCompletedPublishDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgress", x => x.UserId);
                    table.CheckConstraint("CK_UserProgress_NonNegative", "\"LifetimeXp\" >= 0 AND \"CurrentStreak\" >= 0 AND \"LongestStreak\" >= \"CurrentStreak\"");
                    table.ForeignKey(
                        name: "FK_UserProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XpLedger",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpLedger", x => x.Id);
                    table.CheckConstraint("CK_XpLedger_ExactAmount", "\"Amount\" IN (10, 15)");
                    table.ForeignKey(
                        name: "FK_XpLedger_UserAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "UserAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_XpLedger_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecords_ExpiresAt",
                table: "IdempotencyRecords",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempts_ChallengeId_ContentRevisionId",
                table: "UserAttempts",
                columns: new[] { "ChallengeId", "ContentRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempts_DailyMajlisId",
                table: "UserAttempts",
                column: "DailyMajlisId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempts_SelectedOptionId_ChallengeId",
                table: "UserAttempts",
                columns: new[] { "SelectedOptionId", "ChallengeId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempts_UserId_AttemptedAt_Id",
                table: "UserAttempts",
                columns: new[] { "UserId", "AttemptedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "UX_UserAttempts_UserId_DailyMajlisId",
                table: "UserAttempts",
                columns: new[] { "UserId", "DailyMajlisId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpLedger_OccurredAt_Amount",
                table: "XpLedger",
                columns: new[] { "OccurredAt", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_XpLedger_UserId_OccurredAt",
                table: "XpLedger",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "UX_XpLedger_AttemptId",
                table: "XpLedger",
                column: "AttemptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "UserProgress");

            migrationBuilder.DropTable(
                name: "XpLedger");

            migrationBuilder.DropTable(
                name: "UserAttempts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Challenges_Id_RevisionId",
                table: "Challenges");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ChallengeOptions_Id_ChallengeId",
                table: "ChallengeOptions");
        }
    }
}
