using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnnouncmentHub.Migrations
{
    /// <inheritdoc />
    public partial class categories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInMainPage",
                table: "Categories",
                newName: "IsVIP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVIP",
                table: "Categories",
                newName: "IsInMainPage");
        }
    }
}
