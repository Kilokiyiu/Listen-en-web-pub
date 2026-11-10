using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWordIsFavorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "T_UserWord",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "T_UserWord");
        }
    }
}
