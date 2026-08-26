using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityProfileFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AuthenticationNotBefore = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Status", "\"Status\" IN ('active', 'suspended', 'deletion_pending', 'deleted')");
                });

            migrationBuilder.CreateTable(
                name: "AccountDeletionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PurgeDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BackupExpiryDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LegalHoldReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDeletionRequests", x => x.Id);
                    table.CheckConstraint("CK_AccountDeletionRequests_Status", "\"Status\" IN ('requested', 'identity_deleted', 'active_data_purged', 'backup_expiry_pending', 'completed', 'legal_hold')");
                    table.ForeignKey(
                        name: "FK_AccountDeletionRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    DisplayNameNormalized = table.Column<string>(type: "text", nullable: false),
                    AgeBand = table.Column<string>(type: "text", nullable: false),
                    AgeBandAttestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CountryCode = table.Column<string>(type: "character(2)", nullable: true),
                    RegionCode = table.Column<string>(type: "text", nullable: true),
                    DialectCode = table.Column<string>(type: "text", nullable: true),
                    Locale = table.Column<string>(type: "text", nullable: false),
                    LeaderboardVisibility = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                    table.CheckConstraint("CK_Profiles_AgeBand", "\"AgeBand\" IN ('13_17', '18_plus')");
                    table.CheckConstraint("CK_Profiles_CountryCode", "\"CountryCode\" IS NULL OR \"CountryCode\" ~ '^[A-Z]{2}$'");
                    table.CheckConstraint("CK_Profiles_DisplayNameStorageLength", "char_length(btrim(\"DisplayName\")) BETWEEN 1 AND 120");
                    table.CheckConstraint("CK_Profiles_LeaderboardVisibility", "\"LeaderboardVisibility\" IN ('private', 'global_weekly')");
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.CheckConstraint("CK_UserConsents_Type", "\"Type\" IN ('terms', 'privacy', 'analytics')");
                    table.ForeignKey(
                        name: "FK_UserConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Issuer = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    RevocationHandleCiphertext = table.Column<byte[]>(type: "bytea", nullable: true),
                    RevocationKeyVersion = table.Column<string>(type: "text", nullable: true),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAuthenticatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProviderAuthorizationRevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentities", x => x.Id);
                    table.CheckConstraint("CK_UserIdentities_Provider", "\"Provider\" IN ('google', 'apple', 'meta', 'snapchat', 'test')");
                    table.ForeignKey(
                        name: "FK_UserIdentities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderLocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ReminderTimeZoneId = table.Column<string>(type: "text", nullable: true),
                    AnalyticsConsent = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                    table.CheckConstraint("CK_UserRoleAssignments_Role", "\"Role\" IN ('moderator', 'content_editor', 'content_reviewer', 'publisher', 'operations_admin')");
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_UserId",
                table: "AccountDeletionRequests",
                column: "UserId",
                unique: true,
                filter: "\"Status\" <> 'completed'");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId_Type_Version",
                table: "UserConsents",
                columns: new[] { "UserId", "Type", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_Issuer_Subject",
                table: "UserIdentities",
                columns: new[] { "Issuer", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_UserId_Provider",
                table: "UserIdentities",
                columns: new[] { "UserId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_AssignedByUserId",
                table: "UserRoleAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId_Role",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "Role" },
                unique: true,
                filter: "\"RevokedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountDeletionRequests");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropTable(
                name: "UserIdentities");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
