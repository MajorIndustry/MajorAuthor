using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Chapters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "BookInvitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationType",
                table: "BookInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InviteeUserId",
                table: "BookInvitations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookInvitations_InviteeUserId",
                table: "BookInvitations",
                column: "InviteeUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInvitations_AspNetUsers_InviteeUserId",
                table: "BookInvitations",
                column: "InviteeUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInvitations_AspNetUsers_InviteeUserId",
                table: "BookInvitations");

            migrationBuilder.DropIndex(
                name: "IX_BookInvitations_InviteeUserId",
                table: "BookInvitations");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "BookInvitations");

            migrationBuilder.DropColumn(
                name: "InvitationType",
                table: "BookInvitations");

            migrationBuilder.DropColumn(
                name: "InviteeUserId",
                table: "BookInvitations");
        }
    }
}
