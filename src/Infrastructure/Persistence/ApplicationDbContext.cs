using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Domain.Aggregates.Credential; // Alterado de Domain.Aggregates.Secret
using Infrastructure.Data.Context.Configurations;

namespace ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Credential> Credentials => Set<Credential>(); // Alterado de DbSet<Secret> Secrets
    public DbSet<ChavePix> ChavesPix => Set<ChavePix>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Cobranca> Cobrancas => Set<Cobranca>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PixCharge> PixCharges => Set<PixCharge>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conta>().HasKey(c => c.Id);

        // Mapeia a nova classe de agregação Credential
        modelBuilder.Entity<Credential>().HasKey(s => s.Id); // Alterado de Secret para Credential

        // Relacionamento entre Conta e Credential tratado via CredentialId (Foreign Key)
        modelBuilder.Entity<Conta>()
            .Property(c => c.CredentialId) // Alterado de SecretId para CredentialId
            .IsRequired(false);
        
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

        // Aplicar configurações das entidades de domínio
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new PixChargeConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}