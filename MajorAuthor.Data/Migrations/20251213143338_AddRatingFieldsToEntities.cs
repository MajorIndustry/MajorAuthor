using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingFieldsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "Poems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Poems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyRating",
                table: "Poems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "YearlyRating",
                table: "Poems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyRating",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "YearlyRating",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "Blogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Blogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyRating",
                table: "Blogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "Authors",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Authors",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyRating",
                table: "Authors",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "YearlyRating",
                table: "Authors",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "Poems");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Poems");

            migrationBuilder.DropColumn(
                name: "WeeklyRating",
                table: "Poems");

            migrationBuilder.DropColumn(
                name: "YearlyRating",
                table: "Poems");

            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "WeeklyRating",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "YearlyRating",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "WeeklyRating",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "WeeklyRating",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "YearlyRating",
                table: "Authors");
        }
    }
}
