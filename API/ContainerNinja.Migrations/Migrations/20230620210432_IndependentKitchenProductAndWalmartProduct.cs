using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContainerNinja.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class IndependentKitchenProductAndWalmartProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedOrderProducts_Products_ProductId",
                table: "CompletedOrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_KitchenProducts_Products_ProductId",
                table: "KitchenProducts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_KitchenProducts_ProductId",
                table: "KitchenProducts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "CompletedOrderProducts",
                newName: "WalmartProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CompletedOrderProducts_ProductId",
                table: "CompletedOrderProducts",
                newName: "IX_CompletedOrderProducts_WalmartProductId");

            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "KitchenProducts",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "WalmartProductId",
                table: "KitchenProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WalmartProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WalmartId = table.Column<long>(type: "bigint", nullable: true),
                    WalmartLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartItemResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartSearchResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<float>(type: "real", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    KitchenUnitType = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalmartProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WalmartId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_WalmartProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "WalmartProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KitchenProducts_WalmartProductId",
                table: "KitchenProducts",
                column: "WalmartProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedOrderProducts_WalmartProducts_WalmartProductId",
                table: "CompletedOrderProducts",
                column: "WalmartProductId",
                principalTable: "WalmartProducts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenProducts_WalmartProducts_WalmartProductId",
                table: "KitchenProducts",
                column: "WalmartProductId",
                principalTable: "WalmartProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedOrderProducts_WalmartProducts_WalmartProductId",
                table: "CompletedOrderProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_KitchenProducts_WalmartProducts_WalmartProductId",
                table: "KitchenProducts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "WalmartProducts");

            migrationBuilder.DropIndex(
                name: "IX_KitchenProducts_WalmartProductId",
                table: "KitchenProducts");

            migrationBuilder.DropColumn(
                name: "WalmartProductId",
                table: "KitchenProducts");

            migrationBuilder.RenameColumn(
                name: "WalmartProductId",
                table: "CompletedOrderProducts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CompletedOrderProducts_WalmartProductId",
                table: "CompletedOrderProducts",
                newName: "IX_CompletedOrderProducts_ProductId");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "KitchenProducts",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Size = table.Column<float>(type: "real", nullable: false),
                    KitchenUnitType = table.Column<int>(type: "int", nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    WalmartId = table.Column<long>(type: "bigint", nullable: true),
                    WalmartItemResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartSearchResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WalmartSize = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    WalmartId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProducts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KitchenProducts_ProductId",
                table: "KitchenProducts",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedOrderProducts_Products_ProductId",
                table: "CompletedOrderProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenProducts_Products_ProductId",
                table: "KitchenProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
