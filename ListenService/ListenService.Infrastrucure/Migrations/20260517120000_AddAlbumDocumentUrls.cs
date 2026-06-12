using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListenService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumDocumentUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaperFileUrl",
                table: "T_Album",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerFileUrl",
                table: "T_Album",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PaperFileUrl", table: "T_Album");
            migrationBuilder.DropColumn(name: "AnswerFileUrl", table: "T_Album");
        }
    }
}
