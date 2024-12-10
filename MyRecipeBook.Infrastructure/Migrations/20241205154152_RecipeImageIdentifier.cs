using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyRecipeBook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeImageIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageIdentifier",
                table: "Recipes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageIdentifier",
                table: "Recipes");
        }
    }
}
