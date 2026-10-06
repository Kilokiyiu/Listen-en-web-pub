using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddWordPacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_UserWordPackClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordPackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserWordBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimedCount = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserWordPackClaim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_WordPack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordPack", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_WordPackEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordPackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phonetic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Definition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Example = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordPackEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_WordPackEntry_T_WordPack_WordPackId",
                        column: x => x.WordPackId,
                        principalTable: "T_WordPack",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordPackClaim_UserId_WordPackId",
                table: "T_UserWordPackClaim",
                columns: new[] { "UserId", "WordPackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordPackClaim_UserWordBookId",
                table: "T_UserWordPackClaim",
                column: "UserWordBookId");

            migrationBuilder.CreateIndex(
                name: "IX_T_WordPack_Code",
                table: "T_WordPack",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_WordPack_IsPublished_SortOrder",
                table: "T_WordPack",
                columns: new[] { "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_T_WordPackEntry_WordPackId_Rank",
                table: "T_WordPackEntry",
                columns: new[] { "WordPackId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_T_WordPackEntry_WordPackId_Word",
                table: "T_WordPackEntry",
                columns: new[] { "WordPackId", "Word" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_UserWordPackClaim");

            migrationBuilder.DropTable(
                name: "T_WordPackEntry");

            migrationBuilder.DropTable(
                name: "T_WordPack");
        }
    }
}
