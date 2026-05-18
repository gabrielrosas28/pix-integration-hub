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
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<ApiService.Domain.Entities.Charge> Charges => Set<ApiService.Domain.Entities.Charge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conta>().HasKey(c => c.Id);

        // configure Secret primary key and FK relationship via SecretId in Conta
        modelBuilder.Entity<Secret>().HasKey(s => s.SecretID);

        modelBuilder.Entity<Conta>()
            .HasOne(c => c.Secret)
            .WithMany()
            .HasForeignKey(c => c.SecretId)
            .HasPrincipalKey(s => s.SecretID);

        modelBuilder.Entity<ApiService.Domain.Entities.Charge>(b =>
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
            b.ToTable("charges");
        });

        base.OnModelCreating(modelBuilder);
    }
}