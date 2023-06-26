using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class KitchenUnitType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "KitchenProducts",
                newName: "KitchenUnitType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KitchenUnitType",
                table: "KitchenProducts",
                newName: "ProductId");
        }
    }
}
