using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToAuthorPenName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Authors_PenName",
                table: "Authors",
                column: "PenName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Authors_PenName",
                table: "Authors");
        }
    }
}
