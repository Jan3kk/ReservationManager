using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNameAndRenameToLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "table_number",
                table: "restaurant_tables");

            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "restaurant_tables",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "unique_name",
                table: "restaurant_tables",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_tables_unique_name",
                table: "restaurant_tables",
                column: "unique_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_restaurant_tables_unique_name",
                table: "restaurant_tables");

            migrationBuilder.DropColumn(
                name: "label",
                table: "restaurant_tables");

            migrationBuilder.DropColumn(
                name: "unique_name",
                table: "restaurant_tables");

            migrationBuilder.AddColumn<string>(
                name: "table_number",
                table: "restaurant_tables",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
