using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KDemia.Migrations
{
    /// <inheritdoc />
    public partial class VideoCanceled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoPath",
                table: "Courses",
                newName: "VideoUrl");

            migrationBuilder.AddColumn<string>(
                name: "VideoSource",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoSource",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "Courses",
                newName: "VideoPath");
        }
    }
}
