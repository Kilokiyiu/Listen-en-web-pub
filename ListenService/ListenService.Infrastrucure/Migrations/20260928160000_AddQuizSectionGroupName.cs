using ListenService.Infrastrucure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListenService.Infrastrucure.Migrations;

[DbContext(typeof(ListenDbContext))]
[Migration("20260928160000_AddQuizSectionGroupName")]
public partial class AddQuizSectionGroupName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GroupName",
            table: "T_QuizSection",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Section A");

        migrationBuilder.CreateIndex(
            name: "IX_T_QuizSection_AlbumId_GroupName_SequenceNumber",
            table: "T_QuizSection",
            columns: new[] { "AlbumId", "GroupName", "SequenceNumber" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_T_QuizSection_AlbumId_GroupName_SequenceNumber",
            table: "T_QuizSection");

        migrationBuilder.DropColumn(
            name: "GroupName",
            table: "T_QuizSection");
    }
}
