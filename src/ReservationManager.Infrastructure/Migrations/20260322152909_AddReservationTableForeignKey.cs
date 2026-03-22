using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationTableForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_reservations_table_id",
                table: "reservations",
                column: "table_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_restaurant_tables_table_id",
                table: "reservations",
                column: "table_id",
                principalTable: "restaurant_tables",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservations_restaurant_tables_table_id",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_reservations_table_id",
                table: "reservations");
        }
    }
}
