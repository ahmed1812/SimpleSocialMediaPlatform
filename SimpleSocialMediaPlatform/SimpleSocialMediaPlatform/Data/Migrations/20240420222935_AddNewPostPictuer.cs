using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSocialMediaPlatform.Data.Migrations
{
    public partial class AddNewPostPictuer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Posts",
                newName: "PostImageUrl");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostImageUrl",
                table: "Posts",
                newName: "ImageUrl");
        }
    }
}
