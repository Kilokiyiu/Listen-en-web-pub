using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentitySerivce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_AnalyticsDaily",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    UniqueVisitors = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AnalyticsDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_AnalyticsEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AnalyticsEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_AnalyticsDaily_Date",
                table: "T_AnalyticsDaily",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_T_AnalyticsDaily_Date_Path",
                table: "T_AnalyticsDaily",
                columns: new[] { "Date", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_AnalyticsEvent_CreatedAt",
                table: "T_AnalyticsEvent",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_T_AnalyticsEvent_EventType_CreatedAt",
                table: "T_AnalyticsEvent",
                columns: new[] { "EventType", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_AnalyticsDaily");

            migrationBuilder.DropTable(
                name: "T_AnalyticsEvent");
        }
    }
}
