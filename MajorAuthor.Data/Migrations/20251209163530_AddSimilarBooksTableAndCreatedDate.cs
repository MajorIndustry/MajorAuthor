using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSimilarBooksTableAndCreatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimilarBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    SimilarToBookId = table.Column<int>(type: "int", nullable: false),
                    SimilarityScore = table.Column<double>(type: "float(4)", precision: 4, scale: 3, nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CommonGenresCount = table.Column<int>(type: "int", nullable: true),
                    CommonTagsCount = table.Column<int>(type: "int", nullable: true),
                    SameAuthor = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimilarBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimilarBooks_Books_SimilarToBookId",
                        column: x => x.SimilarToBookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimilarBooks_BookId",
                table: "SimilarBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarBooks_BookId_SimilarToBookId",
                table: "SimilarBooks",
                columns: new[] { "BookId", "SimilarToBookId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimilarBooks_SimilarToBookId",
                table: "SimilarBooks",
                column: "SimilarToBookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimilarBooks");
        }
    }
}
