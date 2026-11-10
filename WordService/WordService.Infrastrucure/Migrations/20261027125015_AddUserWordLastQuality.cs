using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordService.Infrastrucure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWordLastQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastQuality",
                table: "T_UserWord",
                type: "int",
                nullable: true);

            // 用最近一次复习记录回填，便于立刻按「忘记/模糊/记住」筛选
            migrationBuilder.Sql(@"
UPDATE w
SET LastQuality = latest.Quality
FROM T_UserWord w
INNER JOIN (
    SELECT WordId, Quality,
           ROW_NUMBER() OVER (PARTITION BY WordId ORDER BY CreationTime DESC) AS rn
    FROM T_WordReviewLog
) latest ON latest.WordId = w.Id AND latest.rn = 1;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastQuality",
                table: "T_UserWord");
        }
    }
}
