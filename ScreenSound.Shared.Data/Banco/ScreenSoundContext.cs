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
    public DbSet<Playlist> Playlists { get; set; }

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

        modelBuilder.Entity<PessoaComAcesso>(pessoa =>
        {
            pessoa.Property(p => p.NomeExibicao).HasMaxLength(60);
            pessoa.Property(p => p.Bio).HasMaxLength(300);
            pessoa.Property(p => p.FotoPerfil).HasMaxLength(300);
            pessoa.Ignore(p => p.NomePublico);
        });

        modelBuilder.Entity<Playlist>(playlist =>
        {
            playlist.Property(p => p.Nome).HasMaxLength(100);
            playlist.Property(p => p.Descricao).HasMaxLength(500);
            playlist.HasOne<PessoaComAcesso>()
                .WithMany()
                .HasForeignKey(p => p.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);
            playlist.HasIndex(p => p.PessoaId);
        });

        modelBuilder.Entity<PlaylistMusica>(item =>
        {
            item.ToTable("PlaylistMusicas");
            item.HasKey(i => new { i.PlaylistId, i.MusicaId });
            item.HasOne(i => i.Playlist)
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
            item.HasOne(i => i.Musica)
                .WithMany()
                .HasForeignKey(i => i.MusicaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);

    }
   

}