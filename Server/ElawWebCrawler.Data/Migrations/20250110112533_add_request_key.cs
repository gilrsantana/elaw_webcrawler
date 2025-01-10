using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElawWebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_request_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestKey",
                table: "HtmlFiles",
                type: "varchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestKey",
                table: "GetDataEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestKey",
                table: "HtmlFiles");

            migrationBuilder.DropColumn(
                name: "RequestKey",
                table: "GetDataEvents");
        }
    }
}
