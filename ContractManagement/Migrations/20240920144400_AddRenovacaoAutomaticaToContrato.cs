using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddRenovacaoAutomaticaToContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RenovacaoAutomatica",
                table: "Contratos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenovacaoAutomatica",
                table: "Contratos");
        }
    }
}
