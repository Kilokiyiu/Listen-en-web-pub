using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_UserWord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Example = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepetitionCount = table.Column<int>(type: "int", nullable: false),
                    EaseFactor = table.Column<double>(type: "float", nullable: false, defaultValue: 2.5),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    NextReview = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserWord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_WordReviewLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordReviewLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_WordRoot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RootId = table.Column<int>(type: "int", nullable: false),
                    Root = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MeaningEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordRoot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_UserWordRootProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordRootId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMastered = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MasteredTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserWordRootProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_UserWordRootProgress_T_WordRoot_WordRootId",
                        column: x => x.WordRootId,
                        principalTable: "T_WordRoot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_WordRootExample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordRootId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Root = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Meaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordRootExample", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_WordRootExample_T_WordRoot_WordRootId",
                        column: x => x.WordRootId,
                        principalTable: "T_WordRoot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_WordRootQuiz",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordRootId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CorrectAnswer = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_WordRootQuiz", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_WordRootQuiz_T_WordRoot_WordRootId",
                        column: x => x.WordRootId,
                        principalTable: "T_WordRoot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWord_UserId_Word",
                table: "T_UserWord",
                columns: new[] { "UserId", "Word" });

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordRootProgress_UserId_WordRootId",
                table: "T_UserWordRootProgress",
                columns: new[] { "UserId", "WordRootId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordRootProgress_WordRootId",
                table: "T_UserWordRootProgress",
                column: "WordRootId");

            migrationBuilder.CreateIndex(
                name: "IX_T_WordReviewLog_UserId_WordId_CreationTime",
                table: "T_WordReviewLog",
                columns: new[] { "UserId", "WordId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_T_WordRoot_RootId",
                table: "T_WordRoot",
                column: "RootId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_WordRootExample_WordRootId",
                table: "T_WordRootExample",
                column: "WordRootId");

            migrationBuilder.CreateIndex(
                name: "IX_T_WordRootQuiz_WordRootId",
                table: "T_WordRootQuiz",
                column: "WordRootId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_UserWord");

            migrationBuilder.DropTable(
                name: "T_UserWordRootProgress");

            migrationBuilder.DropTable(
                name: "T_WordReviewLog");

            migrationBuilder.DropTable(
                name: "T_WordRootExample");

            migrationBuilder.DropTable(
                name: "T_WordRootQuiz");

            migrationBuilder.DropTable(
                name: "T_WordRoot");
        }
    }
}
