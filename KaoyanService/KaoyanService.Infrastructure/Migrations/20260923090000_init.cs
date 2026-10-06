using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaoyanService.Infrastructure.Migrations
{
    [DbContext(typeof(KaoyanDbContext))]
    [Migration("20260923090000_init")]
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 开发中：尚无业务表。库与迁移历史先建好，后续精读实体再增量迁移。
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
