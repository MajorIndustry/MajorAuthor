using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MajorAuthor.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAndAddPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_AspNetUsers_IdentityUserId",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_AspNetUsers_UserId",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogLikes_AspNetUsers_UserId",
                table: "BlogLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookLikes_AspNetUsers_UserId1",
                table: "BookLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReadings_AspNetUsers_UserId1",
                table: "BookReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId1",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerId",
                table: "Followers");

            migrationBuilder.DropForeignKey(
                name: "FK_Followers_AspNetUsers_UserId",
                table: "Followers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Books_BookId",
                table: "Promotions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_UserId",
                table: "UserFavoriteBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_UserId1",
                table: "UserFavoriteBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_UserId",
                table: "UserPreferredGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_UserId1",
                table: "UserPreferredGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId1",
                table: "UserPreferredTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferredTags",
                table: "UserPreferredTags");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferredTags_UserId1",
                table: "UserPreferredTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferredGenres",
                table: "UserPreferredGenres");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferredGenres_UserId1",
                table: "UserPreferredGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteBooks",
                table: "UserFavoriteBooks");

            migrationBuilder.DropIndex(
                name: "IX_UserFavoriteBooks_UserId1",
                table: "UserFavoriteBooks");

            migrationBuilder.DropIndex(
                name: "IX_Followers_UserId",
                table: "Followers");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_BookReadings_UserId1",
                table: "BookReadings");

            migrationBuilder.DropIndex(
                name: "IX_BookLikes_UserId1",
                table: "BookLikes");

            migrationBuilder.DropIndex(
                name: "IX_Authors_IdentityUserId",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserPreferredTags");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserPreferredGenres");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserFavoriteBooks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Followers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BookReadings");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BookReadings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BookLikes");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BookLikes");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Authors");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "UserPreferredTags",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "UserPreferredGenres",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "UserFavoriteBooks",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "LinkUrl",
                table: "Notifications",
                newName: "TargetUrl");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Notifications",
                newName: "Message");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "FollowerId",
                table: "Followers",
                newName: "FollowerApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "Comments",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_UserId1",
                table: "Comments",
                newName: "IX_Comments_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BlogLikes",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogLikes_UserId",
                table: "BlogLikes",
                newName: "IX_BlogLikes_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BlogComments",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "BlogComments",
                newName: "CommentDate");

            migrationBuilder.RenameIndex(
                name: "IX_BlogComments_UserId",
                table: "BlogComments",
                newName: "IX_BlogComments_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "UserPreferredTags",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "UserPreferredGenres",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "UserFavoriteBooks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BookReadings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BookLikes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "BlogLikes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "BlogComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "BlogComments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhotoUrl",
                table: "Authors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PenName",
                table: "Authors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Authors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferredTags",
                table: "UserPreferredTags",
                columns: new[] { "ApplicationUserId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferredGenres",
                table: "UserPreferredGenres",
                columns: new[] { "ApplicationUserId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteBooks",
                table: "UserFavoriteBooks",
                columns: new[] { "ApplicationUserId", "BookId" });

            migrationBuilder.CreateTable(
                name: "ChapterReads",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChapterId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstReadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastReadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterReads", x => new { x.ApplicationUserId, x.ChapterId });
                    table.ForeignKey(
                        name: "FK_ChapterReads_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChapterReads_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Poems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    CommentsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Poems_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoemComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoemId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoemComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoemComments_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoemComments_PoemComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "PoemComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoemComments_Poems_PoemId",
                        column: x => x.PoemId,
                        principalTable: "Poems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoemLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoemId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LikeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoemLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoemLikes_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoemLikes_Poems_PoemId",
                        column: x => x.PoemId,
                        principalTable: "Poems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookReadings_ApplicationUserId",
                table: "BookReadings",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookLikes_ApplicationUserId",
                table: "BookLikes",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogLikes_ApplicationUserId1",
                table: "BlogLikes",
                column: "ApplicationUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComments_ApplicationUserId1",
                table: "BlogComments",
                column: "ApplicationUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_ApplicationUserId",
                table: "Authors",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterReads_ChapterId",
                table: "ChapterReads",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemComments_ApplicationUserId",
                table: "PoemComments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemComments_ParentCommentId",
                table: "PoemComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemComments_PoemId",
                table: "PoemComments",
                column: "PoemId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemLikes_ApplicationUserId",
                table: "PoemLikes",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoemLikes_PoemId",
                table: "PoemLikes",
                column: "PoemId");

            migrationBuilder.CreateIndex(
                name: "IX_Poems_AuthorId",
                table: "Poems",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_AspNetUsers_ApplicationUserId",
                table: "Authors",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_AspNetUsers_ApplicationUserId",
                table: "BlogComments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_AspNetUsers_ApplicationUserId1",
                table: "BlogComments",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments",
                column: "ParentCommentId",
                principalTable: "BlogComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogLikes_AspNetUsers_ApplicationUserId",
                table: "BlogLikes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogLikes_AspNetUsers_ApplicationUserId1",
                table: "BlogLikes",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookLikes_AspNetUsers_ApplicationUserId",
                table: "BookLikes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReadings_AspNetUsers_ApplicationUserId",
                table: "BookReadings",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_ApplicationUserId",
                table: "Comments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerApplicationUserId",
                table: "Followers",
                column: "FollowerApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_ApplicationUserId",
                table: "Notifications",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Books_BookId",
                table: "Promotions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_ApplicationUserId",
                table: "UserFavoriteBooks",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_ApplicationUserId",
                table: "UserPreferredGenres",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_ApplicationUserId",
                table: "UserPreferredTags",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_AspNetUsers_ApplicationUserId",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_AspNetUsers_ApplicationUserId",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_AspNetUsers_ApplicationUserId1",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogLikes_AspNetUsers_ApplicationUserId",
                table: "BlogLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogLikes_AspNetUsers_ApplicationUserId1",
                table: "BlogLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookLikes_AspNetUsers_ApplicationUserId",
                table: "BookLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReadings_AspNetUsers_ApplicationUserId",
                table: "BookReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_ApplicationUserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerApplicationUserId",
                table: "Followers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_ApplicationUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Books_BookId",
                table: "Promotions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_ApplicationUserId",
                table: "UserFavoriteBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_ApplicationUserId",
                table: "UserPreferredGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_ApplicationUserId",
                table: "UserPreferredTags");

            migrationBuilder.DropTable(
                name: "ChapterReads");

            migrationBuilder.DropTable(
                name: "PoemComments");

            migrationBuilder.DropTable(
                name: "PoemLikes");

            migrationBuilder.DropTable(
                name: "Poems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferredTags",
                table: "UserPreferredTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferredGenres",
                table: "UserPreferredGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteBooks",
                table: "UserFavoriteBooks");

            migrationBuilder.DropIndex(
                name: "IX_BookReadings_ApplicationUserId",
                table: "BookReadings");

            migrationBuilder.DropIndex(
                name: "IX_BookLikes_ApplicationUserId",
                table: "BookLikes");

            migrationBuilder.DropIndex(
                name: "IX_BlogLikes_ApplicationUserId1",
                table: "BlogLikes");

            migrationBuilder.DropIndex(
                name: "IX_BlogComments_ApplicationUserId1",
                table: "BlogComments");

            migrationBuilder.DropIndex(
                name: "IX_Authors_ApplicationUserId",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BookReadings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BookLikes");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "BlogLikes");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "BlogComments");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Authors");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "UserPreferredTags",
                newName: "UserId1");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "UserPreferredGenres",
                newName: "UserId1");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "UserFavoriteBooks",
                newName: "UserId1");

            migrationBuilder.RenameColumn(
                name: "TargetUrl",
                table: "Notifications",
                newName: "LinkUrl");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Notifications",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ApplicationUserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameColumn(
                name: "FollowerApplicationUserId",
                table: "Followers",
                newName: "FollowerId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Comments",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ApplicationUserId",
                table: "Comments",
                newName: "IX_Comments_UserId1");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "BlogLikes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogLikes_ApplicationUserId",
                table: "BlogLikes",
                newName: "IX_BlogLikes_UserId");

            migrationBuilder.RenameColumn(
                name: "CommentDate",
                table: "BlogComments",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "BlogComments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogComments_ApplicationUserId",
                table: "BlogComments",
                newName: "IX_BlogComments_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId1",
                table: "UserPreferredTags",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserPreferredTags",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId1",
                table: "UserPreferredGenres",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserPreferredGenres",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId1",
                table: "UserFavoriteBooks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserFavoriteBooks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Followers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "BookReadings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "BookReadings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "BookLikes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "BookLikes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "BlogComments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoUrl",
                table: "Authors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "PenName",
                table: "Authors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Authors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Authors",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferredTags",
                table: "UserPreferredTags",
                columns: new[] { "UserId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferredGenres",
                table: "UserPreferredGenres",
                columns: new[] { "UserId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteBooks",
                table: "UserFavoriteBooks",
                columns: new[] { "UserId", "BookId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferredTags_UserId1",
                table: "UserPreferredTags",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferredGenres_UserId1",
                table: "UserPreferredGenres",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteBooks_UserId1",
                table: "UserFavoriteBooks",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_UserId",
                table: "Followers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReadings_UserId1",
                table: "BookReadings",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookLikes_UserId1",
                table: "BookLikes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_IdentityUserId",
                table: "Authors",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_AspNetUsers_IdentityUserId",
                table: "Authors",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_AspNetUsers_UserId",
                table: "BlogComments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogComments_BlogComments_ParentCommentId",
                table: "BlogComments",
                column: "ParentCommentId",
                principalTable: "BlogComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogLikes_AspNetUsers_UserId",
                table: "BlogLikes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookLikes_AspNetUsers_UserId1",
                table: "BookLikes",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReadings_AspNetUsers_UserId1",
                table: "BookReadings",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId1",
                table: "Comments",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_AspNetUsers_FollowerId",
                table: "Followers",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Followers_AspNetUsers_UserId",
                table: "Followers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Books_BookId",
                table: "Promotions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_UserId",
                table: "UserFavoriteBooks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteBooks_AspNetUsers_UserId1",
                table: "UserFavoriteBooks",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_UserId",
                table: "UserPreferredGenres",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredGenres_AspNetUsers_UserId1",
                table: "UserPreferredGenres",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId",
                table: "UserPreferredTags",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferredTags_AspNetUsers_UserId1",
                table: "UserPreferredTags",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
