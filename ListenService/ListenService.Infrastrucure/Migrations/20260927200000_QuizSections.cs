using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListenService.Infrastrucure.Migrations
{
    [DbContext(typeof(ListenDbContext))]
    [Migration("20260927200000_QuizSections")]
    public partial class QuizSections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_QuizSection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationInSecond = table.Column<double>(type: "float", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_QuizSection", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_QuizSection_AlbumId_IsVisible",
                table: "T_QuizSection",
                columns: new[] { "AlbumId", "IsVisible" });

            migrationBuilder.CreateIndex(
                name: "IX_T_QuizSection_AlbumId_SequenceNumber",
                table: "T_QuizSection",
                columns: new[] { "AlbumId", "SequenceNumber" });

            // 旧版按 Album 挂题 → 重建为按 Section 挂题（开发期无历史数据依赖）
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[T_QuizQuestion]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [T_QuizQuestion];
END
");

            migrationBuilder.CreateTable(
                name: "T_QuizQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "IX_T_QuizQuestion_SectionId_IsVisible",
                table: "T_QuizQuestion",
                columns: new[] { "SectionId", "IsVisible" });

            migrationBuilder.CreateIndex(
                name: "IX_T_QuizQuestion_SectionId_Number",
                table: "T_QuizQuestion",
                columns: new[] { "SectionId", "Number" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "T_QuizQuestion");
            migrationBuilder.DropTable(name: "T_QuizSection");
        }
    }
}
