using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Modelos;
using ScreenSound.Shared.Modelos.Avaliacao;

namespace ScreenSound.Banco;


public class ScreenSoundContext : IdentityDbContext<PessoaComAcesso, PerfilDeAcesso, int>
{

    public DbSet<Artista> Artistas { get; set; }
    public DbSet<Musica> Musicas { get; set; }
    public DbSet<Album> Albuns { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<Genero> Generos { get; set; }

    public ScreenSoundContext(DbContextOptions<ScreenSoundContext> options) : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;            
        }
        optionsBuilder
            .UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Musica>().HasMany(c => c.Generos).WithMany(c => c.Musicas);
        modelBuilder.Entity<Album>().HasMany(c => c.Musicas).WithOne(c => c.Album);
        modelBuilder.Entity<Artista>().HasMany(c => c.Albuns).WithOne(c => c.Artista);
        base.OnModelCreating(modelBuilder);

    }
   

}