using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TurismoRural.Models;

namespace TurismoRural.Context;

public partial class TurismoContext : DbContext
{
    public TurismoContext()
    {
    }

    public TurismoContext(DbContextOptions<TurismoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Avaliacao> Avaliacao { get; set; }

    public virtual DbSet<Casa> Casa { get; set; }

    public virtual DbSet<Casa_img> Casa_img { get; set; }

    public virtual DbSet<CodigoPostal> CodigoPostal { get; set; }

    public virtual DbSet<Distrito> Distrito { get; set; }

    public virtual DbSet<Localidade> Localidade { get; set; }

    public virtual DbSet<Reserva> Reserva { get; set; }

    public virtual DbSet<Utilizador> Utilizador { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ProjVerao;User Id=admin;Password=1234;TrustServerCertificate=True;MultipleActiveResultSets=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Avaliacao>(entity =>
        {
            entity.HasKey(e => e.AvaliacaoID).HasName("PK__Avaliaca__FC95FF3881CA023E");

            entity.Property(e => e.DataAvaliacao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Casa).WithMany(p => p.Avaliacao).HasConstraintName("FK__Avaliacao__CasaI__2B0A656D");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Avaliacao).HasConstraintName("FK__Avaliacao__Utili__2BFE89A6");
        });

        modelBuilder.Entity<Casa>(entity =>
        {
            entity.HasKey(e => e.CasaID).HasName("PK__Casa__6DAED36A0B605A22");

            entity.HasOne(d => d.CodigoPostalNavigation).WithMany(p => p.Casa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Casa__CodigoPost__1F98B2C1");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Casa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Casa__Utilizador__208CD6FA");
        });

        modelBuilder.Entity<Casa_img>(entity =>
        {
            entity.HasKey(e => e.ImagemID).HasName("PK__Casa_img__0CBF2ACE8D5448DA");

            entity.HasOne(d => d.Casa).WithMany(p => p.Casa_img).HasConstraintName("FK__Casa_img__CasaID__236943A5");
        });

        modelBuilder.Entity<CodigoPostal>(entity =>
        {
            entity.HasKey(e => e.CodigoPostal1).HasName("PK__CodigoPo__B3F469746AA6691F");

            entity.HasOne(d => d.Localidade).WithMany(p => p.CodigoPostal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CodigoPos__Local__19DFD96B");
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.DistritoID).HasName("PK__Distrito__BE6ADABD0AC0968B");

            entity.Property(e => e.DistritoID).ValueGeneratedNever();
        });

        modelBuilder.Entity<Localidade>(entity =>
        {
            entity.HasKey(e => e.LocalidadeID).HasName("PK__Localida__4C0E1EAA6DDD6ACD");

            entity.Property(e => e.LocalidadeID).ValueGeneratedNever();

            entity.HasOne(d => d.Distrito).WithMany(p => p.Localidade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Localidad__Distr__17036CC0");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.ReservaID).HasName("PK__Reserva__C3993703F0F46B2B");

            entity.HasOne(d => d.Casa).WithMany(p => p.Reserva).HasConstraintName("FK__Reserva__CasaID__2645B050");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Reserva).HasConstraintName("FK__Reserva__Utiliza__2739D489");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.UtilizadorID).HasName("PK__Utilizad__90F8E1C8B0079326");

            entity.Property(e => e.IsSupport).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
