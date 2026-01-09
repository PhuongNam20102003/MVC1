using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thuongmaidientu.Migrations
{
    /// <inheritdoc />
    public partial class AddPaypalOrderIdToHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaypalOrderId",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaypalOrderId",
                table: "HoaDon");
        }
    }
}
