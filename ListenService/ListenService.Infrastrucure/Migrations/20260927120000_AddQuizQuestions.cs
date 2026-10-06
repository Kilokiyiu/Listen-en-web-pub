using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListenService.Infrastrucure.Migrations
{
    [DbContext(typeof(ListenDbContext))]
    [Migration("20260927120000_AddQuizQuestions")]
    public partial class AddQuizQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_QuizQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Stem = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_QuizQuestion", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_QuizQuestion_AlbumId_IsVisible",
                table: "T_QuizQuestion",
                columns: new[] { "AlbumId", "IsVisible" });

            migrationBuilder.CreateIndex(
                name: "IX_T_QuizQuestion_AlbumId_Number",
                table: "T_QuizQuestion",
                columns: new[] { "AlbumId", "Number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "T_QuizQuestion");
        }
    }
}
