using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProductToWalmartProductFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_WalmartProducts_ProductId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "OrderItems",
                newName: "WalmartProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                newName: "IX_OrderItems_WalmartProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_WalmartProducts_WalmartProductId",
                table: "OrderItems",
                column: "WalmartProductId",
                principalTable: "WalmartProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_WalmartProducts_WalmartProductId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "WalmartProductId",
                table: "OrderItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_WalmartProductId",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_WalmartProducts_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "WalmartProducts",
                principalColumn: "Id");
        }
    }
}
