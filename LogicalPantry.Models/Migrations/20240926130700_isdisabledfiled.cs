using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogicalPantry.Models.Migrations
{
    /// <inheritdoc />
    public partial class isdisabledfiled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Users",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Users");
        }
    }
}
