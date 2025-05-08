using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenSound.Migrations
{
    /// <inheritdoc />
    public partial class ReinstituirTabelas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "Albuns",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                   ArtistaId = table.Column<int>(type: "int", nullable: true),
                   DuracaoTotal = table.Column<int>(type: "int", nullable: true)  // Caso queira armazenar a duração total, embora seja uma propriedade calculada
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_Albuns", x => x.Id);
                   table.ForeignKey(
                       name: "FK_Albuns_Artistas_ArtistaId",
                       column: x => x.ArtistaId,
                       principalTable: "Artistas",
                       principalColumn: "Id");
               });
            /// <inheritdoc />
            migrationBuilder.CreateTable(
                 name: "Musicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duracao = table.Column<int>(type: "int", nullable: true),
                    ArtistaId = table.Column<int>(type: "int", nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: true),
                    AnoLancamento = table.Column<int>(type: "int", nullable: true),
                    Disponivel = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Musicas_Albuns_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albuns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Musicas_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id");
                });

            // Criar a tabela Generos
            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            // Criar a tabela MusicaGenero (tabela de junção para o relacionamento muitos-para-muitos)
            migrationBuilder.CreateTable(
                name: "GeneroMusica",
                columns: table => new
                {
                    GenerosId = table.Column<int>(type: "int", nullable: false),
                    MusicasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneroMusica", x => new { x.GenerosId, x.MusicasId });
                    table.ForeignKey(
                        name: "FK_GeneroMusica_Generos_GenerosId",
                        column: x => x.GenerosId,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneroMusica_Musicas_MusicasId",
                        column: x => x.MusicasId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Criar a tabela Avaliacoes
            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    PessoaId = table.Column<int>(type: "int", nullable: false),
                    AlbumId = table.Column<int>(type: "int", nullable: true),
                    ArtistaId = table.Column<int>(type: "int", nullable: true),
                    MusicaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Albuns_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albuns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover as tabelas em ordem inversa
            migrationBuilder.DropTable(name: "Avaliacoes");
            migrationBuilder.DropTable(name: "MusicaGenero");
            migrationBuilder.DropTable(name: "Generos");
            migrationBuilder.DropTable(name: "Musicas");
            migrationBuilder.DropTable(name: "Albuns");
        }
    }
}
    
