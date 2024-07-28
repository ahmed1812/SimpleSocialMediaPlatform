using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSocialMediaPlatform.Data.Migrations
{
    public partial class AddAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(" INSERT INTO [security].[Users] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Address], [Address2], [City], [CreateAt], [DOB], [FullName], [Phone], [ProfilePicture], [State], [ZipCode]) VALUES (N'7334a55f-c75f-4e9a-ad5b-4eb6c8fecae8', N'admin', N'ADMIN', N'admin@test.com', N'ADMIN@TEST.COM', 1, N'AQAAAAEAACcQAAAAEBj9LHMaHRvT1hv8n5DQjnSglM8m6xaX/IMem/QhaStLjF+4aJez4K7Ixin3TIOKoQ==', N'2V4DNVSQVAPRCVFQMOBPA7VJKQCQ6HAZ', N'7a15e996-a623-4b0f-ab4e-063b4cd2f5d7', NULL, 0, 0, NULL, 1, 0, N'', N'', N'', N'0001-01-01 00:00:00', N'2024-07-27 00:00:00', N'asdasdas', N'', NULL, N'', N'')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [security].[Users] WHERE Id = '7334a55f-c75f-4e9a-ad5b-4eb6c8fecae8'");
        }
    }
}
