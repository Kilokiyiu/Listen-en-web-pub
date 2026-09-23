using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentitySerivce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_StudyActivity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_StudyActivity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_StudyActivity_UserId_ActivityType_ContentId",
                table: "T_StudyActivity",
                columns: new[] { "UserId", "ActivityType", "ContentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_StudyActivity_UserId_UpdatedAt",
                table: "T_StudyActivity",
                columns: new[] { "UserId", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_StudyActivity");
        }
    }
}
