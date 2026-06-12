using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudySummarizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultipleSummariesAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Summaries_Documents_DocumentId",
                table: "Summaries");

            migrationBuilder.DropIndex(
                name: "IX_Summaries_DocumentId",
                table: "Summaries");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Documents",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Documents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Summaries_DocumentId",
                table: "Summaries",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Summaries_Documents_DocumentId",
                table: "Summaries",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Summaries_Documents_DocumentId",
                table: "Summaries");

            migrationBuilder.DropIndex(
                name: "IX_Summaries_DocumentId",
                table: "Summaries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Documents",
                newName: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Summaries_DocumentId",
                table: "Summaries",
                column: "DocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Summaries_Documents_DocumentId",
                table: "Summaries",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
