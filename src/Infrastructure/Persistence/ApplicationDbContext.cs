using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Infrastructure.Data.Context.Configurations;

namespace ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<PixKey> PixKeys => Set<PixKey>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PixCharge> PixCharges => Set<PixCharge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(a => a.Id);

        modelBuilder.Entity<Credential>().HasKey(c => c.Id);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(a => a.CredentialId).IsRequired(false);

            entity.HasOne(a => a.Credential)
                  .WithMany()
                  .HasForeignKey(a => a.CredentialId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<PixKey>()
            .HasOne(p => p.Account)
            .WithMany()
            .HasForeignKey(p => p.AccountId);

        modelBuilder.Entity<Charge>(b =>
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

        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new PixChargeConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
