using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElawWebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class html_file : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HtmlFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    FileName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    FileContentAddress = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    FileUrl = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlFiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HtmlFiles");
        }
    }
}
