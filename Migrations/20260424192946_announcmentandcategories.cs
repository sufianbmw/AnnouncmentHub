using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnnouncmentHub.Migrations
{
    /// <inheritdoc />
    public partial class announcmentandcategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInMainPage",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVIP",
                table: "Announcements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInMainPage",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsVIP",
                table: "Announcements");
        }
    }
}
