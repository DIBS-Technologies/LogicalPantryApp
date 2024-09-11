using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogicalPantry.Models.Migrations
{
    /// <inheritdoc />
    public partial class EventTypeAndNumberOfUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "TimeSlots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxNumberOfUsers",
                table: "TimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "MaxNumberOfUsers",
                table: "TimeSlots");
        }
    }
}
