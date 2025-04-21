using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module35.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFriendRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendRelations_AspNetUsers_FriendId",
                table: "FriendRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRelations_AspNetUsers_UserId",
                table: "FriendRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRelations_AspNetUsers_FriendId",
                table: "FriendRelations",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRelations_AspNetUsers_UserId",
                table: "FriendRelations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendRelations_AspNetUsers_FriendId",
                table: "FriendRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRelations_AspNetUsers_UserId",
                table: "FriendRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRelations_AspNetUsers_FriendId",
                table: "FriendRelations",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRelations_AspNetUsers_UserId",
                table: "FriendRelations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
