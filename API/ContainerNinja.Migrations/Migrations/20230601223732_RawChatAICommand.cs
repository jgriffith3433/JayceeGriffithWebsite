using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RawChatAICommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseToUser",
                table: "ChatCommands");

            migrationBuilder.DropColumn(
                name: "SystemResponse",
                table: "ChatCommands");

            migrationBuilder.RenameColumn(
                name: "RawReponse",
                table: "ChatCommands",
                newName: "RawChatAICommand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RawChatAICommand",
                table: "ChatCommands",
                newName: "RawReponse");

            migrationBuilder.AddColumn<string>(
                name: "ResponseToUser",
                table: "ChatCommands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemResponse",
                table: "ChatCommands",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
