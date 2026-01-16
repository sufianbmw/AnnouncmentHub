using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnnouncmentHub.Migrations
{
    /// <inheritdoc />
    public partial class iscoveredforimages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCovered",
                table: "AnnouncementImage",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCovered",
                table: "AnnouncementImage");
        }
    }
}
