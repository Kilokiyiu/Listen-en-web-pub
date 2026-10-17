using System;
using ListenService.Infrastrucure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListenService.Infrastrucure.Migrations;

[DbContext(typeof(ListenDbContext))]
[Migration("20261015080000_AddAnnouncementAndSiteRuntime")]
public partial class AddAnnouncementAndSiteRuntime : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "T_Announcement",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false),
                IsPinned = table.Column<bool>(type: "bit", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_T_Announcement", x => x.Id)
                    .Annotation("SqlServer:Clustered", false);
            });

        migrationBuilder.CreateIndex(
            name: "IX_T_Announcement_IsPublished_IsPinned_SortOrder_PublishDate",
            table: "T_Announcement",
            columns: new[] { "IsPublished", "IsPinned", "SortOrder", "PublishDate" });

        migrationBuilder.CreateTable(
            name: "T_SiteRuntimeInfo",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                StatusText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ExtraInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_T_SiteRuntimeInfo", x => x.Id)
                    .Annotation("SqlServer:Clustered", false);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "T_Announcement");
        migrationBuilder.DropTable(name: "T_SiteRuntimeInfo");
    }
}
