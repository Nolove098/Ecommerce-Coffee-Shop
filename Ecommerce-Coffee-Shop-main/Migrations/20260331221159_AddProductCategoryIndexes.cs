using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaleStore.Migrations
{
    public partial class AddProductCategoryIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "products",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Category",
                table: "products",
                columns: new[] { "is_active", "category" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Category",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Category",
                table: "products");
        }
    }
}
