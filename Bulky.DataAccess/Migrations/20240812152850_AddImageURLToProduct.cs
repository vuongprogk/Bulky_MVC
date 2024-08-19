using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddImageURLToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CatagoryID",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Products",
                type: "longtext",
                nullable: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 1, "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 2, "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 3, "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 1, "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 2, "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CatagoryID", "ImageURL" },
                values: new object[] { 3, "" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatagoryID",
                table: "Products",
                column: "CatagoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Catagories_CatagoryID",
                table: "Products",
                column: "CatagoryID",
                principalTable: "Catagories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Catagories_CatagoryID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CatagoryID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatagoryID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Products");
        }
    }
}
