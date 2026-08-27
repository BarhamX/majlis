using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeOptionOwnershipIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChallengeOptions_ChallengeId",
                table: "ChallengeOptions");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeOptions_OptionKey_SortOrder",
                table: "ChallengeOptions");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_ChallengeId_OptionKey",
                table: "ChallengeOptions",
                columns: new[] { "ChallengeId", "OptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_ChallengeId_SortOrder",
                table: "ChallengeOptions",
                columns: new[] { "ChallengeId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChallengeOptions_ChallengeId_OptionKey",
                table: "ChallengeOptions");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeOptions_ChallengeId_SortOrder",
                table: "ChallengeOptions");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_ChallengeId",
                table: "ChallengeOptions",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_OptionKey_SortOrder",
                table: "ChallengeOptions",
                columns: new[] { "OptionKey", "SortOrder" });
        }
    }
}
