using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSocialMediaPlatform.Data.Migrations
{
    public partial class AddNewPostImageName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageName",
                table: "Posts",
                newName: "PostImageName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostImageName",
                table: "Posts",
                newName: "ImageName");
        }
    }
}
