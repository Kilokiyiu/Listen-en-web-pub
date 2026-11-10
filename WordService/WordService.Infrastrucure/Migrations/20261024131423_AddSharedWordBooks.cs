using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedWordBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_SharedWordBook",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceWordBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SharedWordBook", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_SharedWordBookEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedWordBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Example = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_SharedWordBookEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_SharedWordBookEntry_T_SharedWordBook_SharedWordBookId",
                        column: x => x.SharedWordBookId,
                        principalTable: "T_SharedWordBook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_SharedWordBook_Code",
                table: "T_SharedWordBook",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_SharedWordBook_OwnerUserId_SourceWordBookId",
                table: "T_SharedWordBook",
                columns: new[] { "OwnerUserId", "SourceWordBookId" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SharedWordBookEntry_SharedWordBookId_Rank",
                table: "T_SharedWordBookEntry",
                columns: new[] { "SharedWordBookId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_T_SharedWordBookEntry_SharedWordBookId_Word",
                table: "T_SharedWordBookEntry",
                columns: new[] { "SharedWordBookId", "Word" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_SharedWordBookEntry");

            migrationBuilder.DropTable(
                name: "T_SharedWordBook");
        }
    }
}
