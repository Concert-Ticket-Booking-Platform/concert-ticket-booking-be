using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlAndConcertImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "concerts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "concerts");
        }
    }
}
