using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorrecolTest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Pais",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "DivisionPoliticaColombia",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "DepartamentosColombia",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Pais");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "DivisionPoliticaColombia");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "DepartamentosColombia");
        }
    }
}
