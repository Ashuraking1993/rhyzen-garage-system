using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car_Rental.Migrations
{
    /// <inheritdoc />
    public partial class ForceAddBookingCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingCode",
                table: "Bookings",
                type: "nvarchar(7)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingCode",
                table: "Bookings");
        }
    }
}
