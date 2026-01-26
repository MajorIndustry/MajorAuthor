using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReadingForPoemAndBlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogReadings_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlogReadings_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoemReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoemId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoemReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoemReadings_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoemReadings_Poems_PoemId",
                        column: x => x.PoemId,
                        principalTable: "Poems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogReadings_ApplicationUserId",
                table: "BlogReadings",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogReadings_BlogId",
                table: "BlogReadings",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemReadings_ApplicationUserId",
                table: "PoemReadings",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemReadings_PoemId",
                table: "PoemReadings",
                column: "PoemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogReadings");

            migrationBuilder.DropTable(
                name: "PoemReadings");
        }
    }
}
