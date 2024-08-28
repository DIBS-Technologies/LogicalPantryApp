using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogicalPantry.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantDisplayName",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantDisplayName",
                table: "Tenants");
        }
    }
}
