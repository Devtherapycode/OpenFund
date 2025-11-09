using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenFund.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRemoveYoutubeChannelIdAddIsCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YoutubeChannelId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsCreator",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCreator",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YoutubeChannelId",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
