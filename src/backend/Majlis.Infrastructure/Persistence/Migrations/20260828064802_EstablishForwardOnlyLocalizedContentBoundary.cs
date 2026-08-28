using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Majlis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EstablishForwardOnlyLocalizedContentBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Localized content revisions are a forward-only boundary; " +
                "restore a compatible backup or apply a reviewed forward recovery migration.");
        }
    }
}
