using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaoyanService.Infrastructure.Migrations
{
    [DbContext(typeof(KaoyanDbContext))]
    [Migration("20261023120000_AddExamPaperDocumentUrls")]
    public partial class AddExamPaperDocumentUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaperFileUrl",
                table: "T_ExamPaper",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerFileUrl",
                table: "T_ExamPaper",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PaperFileUrl", table: "T_ExamPaper");
            migrationBuilder.DropColumn(name: "AnswerFileUrl", table: "T_ExamPaper");
        }
    }
}
