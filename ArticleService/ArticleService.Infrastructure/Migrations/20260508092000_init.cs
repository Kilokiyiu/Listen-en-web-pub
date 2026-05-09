using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_DailyArticle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title_Chinese = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title_English = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    EnglishText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChineseText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArticleUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_DailyArticle", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "T_UserArticleStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsFavorited = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserArticleStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_UserArticleStatus_T_DailyArticle_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "T_DailyArticle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_DailyArticle_PublicDate",
                table: "T_DailyArticle",
                column: "PublicDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_UserArticleStatus_ArticleId",
                table: "T_UserArticleStatus",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserArticleStatus_UserId_ArticleId",
                table: "T_UserArticleStatus",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_UserArticleStatus");

            migrationBuilder.DropTable(
                name: "T_DailyArticle");
        }
    }
}
