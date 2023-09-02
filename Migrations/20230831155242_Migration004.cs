using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugscapeMVC.Migrations
{
    /// <inheritdoc />
    public partial class Migration004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_ProjectId",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Invites",
                newName: "Role");

            migrationBuilder.AlterColumn<string>(
                name: "InvitorId",
                table: "Invites",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Invites",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites",
                column: "InvitorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Invites",
                newName: "ProjectId");

            migrationBuilder.AlterColumn<string>(
                name: "InvitorId",
                table: "Invites",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_ProjectId",
                table: "Invites",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites",
                column: "InvitorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
