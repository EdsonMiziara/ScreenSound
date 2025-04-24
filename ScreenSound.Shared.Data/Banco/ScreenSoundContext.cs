using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScreenSound.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ScreenSound.Banco;


public class ScreenSoundContext : DbContext
{

    public DbSet<Artista> Artistas { get; set; }
    public DbSet<Musica> Musicas { get; set; }
    public DbSet<Album> Albuns { get; set; }
    public DbSet<Avaliacao> Avaliacao { get; set; }
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
    }

}