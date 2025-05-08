using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Modelos;
using ScreenSound.Shared.Modelos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ScreenSound.Banco;


public class ScreenSoundContext : IdentityDbContext<PessoaComAcesso, PerfilDeAcesso, int>
{

    public DbSet<Artista> Artistas { get; set; }
    public DbSet<Musica> Musicas { get; set; }
    public DbSet<Album> Albuns { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<Genero> Generos { get; set; }

    //public string connectionstring = "data source=(localdb)\\mssqllocaldb;initial catalog=screensoundv0;integrated security=true;encrypt=false;trust server certificate=false;application intent=readwrite;multi subnet failover=false";

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