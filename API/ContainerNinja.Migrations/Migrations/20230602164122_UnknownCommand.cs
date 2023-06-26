using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class UnknownCommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Unknown",
                table: "ChatCommands",
                newName: "UnknownCommand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnknownCommand",
                table: "ChatCommands",
                newName: "Unknown");
        }
    }
}
