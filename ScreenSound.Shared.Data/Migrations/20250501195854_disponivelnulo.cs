using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenSound.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class disponivelNulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
               name: "Disponivel",
               table: "Musicas",
               nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
               name: "Disponivel",
               table: "Musicas",
               nullable: false,
               defaultValue: false);
        }
    }
}
