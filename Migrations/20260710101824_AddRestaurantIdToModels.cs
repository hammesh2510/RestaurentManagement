using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantIdToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RestaurantId",
                table: "RestaurantTables",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RestaurantId",
                table: "MenuItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RestaurantId",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Categories");
        }
    }
}
