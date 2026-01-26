using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookTypesAndCycles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowDownload",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CycleId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTTS",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BookCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookCycles_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_CycleId",
                table: "Books",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_StatusId",
                table: "Books",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_TypeId",
                table: "Books",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCycles_AuthorId",
                table: "BookCycles",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookCycles_CycleId",
                table: "Books",
                column: "CycleId",
                principalTable: "BookCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookStatuses_StatusId",
                table: "Books",
                column: "StatusId",
                principalTable: "BookStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookTypes_TypeId",
                table: "Books",
                column: "TypeId",
                principalTable: "BookTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookCycles_CycleId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookStatuses_StatusId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookTypes_TypeId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "BookCycles");

            migrationBuilder.DropTable(
                name: "BookStatuses");

            migrationBuilder.DropTable(
                name: "BookTypes");

            migrationBuilder.DropIndex(
                name: "IX_Books_CycleId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_StatusId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_TypeId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "AllowDownload",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CycleId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "EnableTTS",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Books");
        }
    }
}
