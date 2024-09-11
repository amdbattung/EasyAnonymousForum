using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyAnonymousForum.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_topics_name",
                table: "topics",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "ix_comments_content",
                table: "comments",
                column: "content")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_topics_name",
                table: "topics");

            migrationBuilder.DropIndex(
                name: "ix_comments_content",
                table: "comments");
        }
    }
}
