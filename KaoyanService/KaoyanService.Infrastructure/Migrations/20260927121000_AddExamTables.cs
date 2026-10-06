using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaoyanService.Infrastructure.Migrations
{
    [DbContext(typeof(KaoyanDbContext))]
    [Migration("20260927121000_AddExamTables")]
    public partial class AddExamTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_ExamPaper",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Series = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ExamPaper", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "T_ExamSection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Passage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ExamSection", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "T_ExamQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Stem = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ExamQuestion", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_ExamPaper_IsVisible",
                table: "T_ExamPaper",
                column: "IsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_T_ExamPaper_Year_Series",
                table: "T_ExamPaper",
                columns: new[] { "Year", "Series" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ExamSection_PaperId_SequenceNumber",
                table: "T_ExamSection",
                columns: new[] { "PaperId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_T_ExamQuestion_SectionId_Number",
                table: "T_ExamQuestion",
                columns: new[] { "SectionId", "Number" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "T_ExamQuestion");
            migrationBuilder.DropTable(name: "T_ExamSection");
            migrationBuilder.DropTable(name: "T_ExamPaper");
        }
    }
}
