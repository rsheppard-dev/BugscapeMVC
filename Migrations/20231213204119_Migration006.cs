using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugscapeMVC.Migrations
{
    /// <inheritdoc />
    public partial class Migration006 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Invites",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_ProjectId",
                table: "Invites",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_ProjectId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Invites");
        }
    }
}
