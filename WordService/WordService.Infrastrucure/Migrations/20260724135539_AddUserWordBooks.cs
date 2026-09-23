using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWordBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_UserWord_UserId_Word",
                table: "T_UserWord");

            migrationBuilder.CreateTable(
                name: "T_UserWordBook",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserWordBook", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "WordBookId",
                table: "T_UserWord",
                type: "uniqueidentifier",
                nullable: true);

            // 为已有单词的用户创建默认单词本，并回填 WordBookId
            migrationBuilder.Sql("""
                INSERT INTO T_UserWordBook (Id, UserId, Name, Description, IsDefault, SortOrder, CreationTime)
                SELECT NEWID(), u.UserId, N'默认单词本', NULL, 1, 0, GETDATE()
                FROM (SELECT DISTINCT UserId FROM T_UserWord) u;

                UPDATE uw
                SET uw.WordBookId = b.Id
                FROM T_UserWord uw
                INNER JOIN T_UserWordBook b ON b.UserId = uw.UserId AND b.IsDefault = 1
                WHERE uw.WordBookId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "WordBookId",
                table: "T_UserWord",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWord_UserId_WordBookId_Word",
                table: "T_UserWord",
                columns: new[] { "UserId", "WordBookId", "Word" });

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWord_WordBookId",
                table: "T_UserWord",
                column: "WordBookId");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordBook_UserId_IsDefault",
                table: "T_UserWordBook",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWordBook_UserId_Name",
                table: "T_UserWordBook",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_T_UserWord_T_UserWordBook_WordBookId",
                table: "T_UserWord",
                column: "WordBookId",
                principalTable: "T_UserWordBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_UserWord_T_UserWordBook_WordBookId",
                table: "T_UserWord");

            migrationBuilder.DropTable(
                name: "T_UserWordBook");

            migrationBuilder.DropIndex(
                name: "IX_T_UserWord_UserId_WordBookId_Word",
                table: "T_UserWord");

            migrationBuilder.DropIndex(
                name: "IX_T_UserWord_WordBookId",
                table: "T_UserWord");

            migrationBuilder.DropColumn(
                name: "WordBookId",
                table: "T_UserWord");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserWord_UserId_Word",
                table: "T_UserWord",
                columns: new[] { "UserId", "Word" });
        }
    }
}
