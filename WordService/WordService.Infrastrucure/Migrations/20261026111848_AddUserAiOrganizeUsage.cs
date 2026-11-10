using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAiOrganizeUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_UserAiOrganizeUsage",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUsedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserAiOrganizeUsage", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_UserAiOrganizeUsage");
        }
    }
}
