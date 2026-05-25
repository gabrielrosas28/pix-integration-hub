using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Secret> Secrets => Set<Secret>();
    public DbSet<ChavePix> ChavesPix => Set<ChavePix>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Cobranca> Cobrancas => Set<Cobranca>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conta>().HasKey(c => c.Id);

        modelBuilder.Entity<Secret>().HasKey(s => s.Id);

        modelBuilder.Entity<Conta>()
            .HasOne(c => c.Secret)
            .WithMany()
            .HasForeignKey(c => c.SecretId)
            .HasPrincipalKey(s => s.Id);
        
        modelBuilder.Entity<ChavePix>()
            .HasOne(cp => cp.Conta)
            .WithMany()
            .HasForeignKey(cp => cp.ContaId);

        modelBuilder.Entity<Cobranca>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.TxId).IsRequired();
            b.Property(c => c.InvoiceID);
            b.Property(c => c.ChargeType);
            b.Property(c => c.Amount).HasColumnType("numeric");
            b.Property(c => c.PixKey);
            b.Property(c => c.Emv);
            b.Property(c => c.PixLink);
            b.Property(c => c.Status);
            b.Property(c => c.CreatedAt);
            b.ToTable("cobrancas");
        });

        base.OnModelCreating(modelBuilder);
    }
}