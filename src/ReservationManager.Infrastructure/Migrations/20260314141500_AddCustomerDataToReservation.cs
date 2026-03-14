using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDataToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "party_size",
                table: "reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "party_size",
                table: "reservations");
        }
    }
}
