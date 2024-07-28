using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSocialMediaPlatform.Data.Migrations
{
    public partial class AssignAdminUserToAllRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [security].[UserRoles] (UserId, RoleId) SELECT '7334a55f-c75f-4e9a-ad5b-4eb6c8fecae8', Id FROM [security].[Roles]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [security].[UserRoles] WHERE UserId = '7334a55f-c75f-4e9a-ad5b-4eb6c8fecae8'");
        }
    }
}
