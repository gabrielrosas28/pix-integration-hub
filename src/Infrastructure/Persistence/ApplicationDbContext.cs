using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Domain.Aggregates.Credential; // Alterado de Domain.Aggregates.Secret
using Infrastructure.Data.Context.Configurations;
using Application.Interfaces;
using Credential = Domain.Aggregates.Credential.Credential;

namespace ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Contas => Set<Account>();
    public DbSet<Credential> Credentials => Set<Credential>(); // Alterado de DbSet<Secret> Secrets
    public DbSet<PixKey> ChavesPix => Set<PixKey>();
    public DbSet<Audit> Auditorias => Set<Audit>();
    public DbSet<Charge> Cobrancas => Set<Charge>();
    // Invoice/PixCharge: agregados DDD ainda nao usados por nenhum endpoint registrado
    // e com mapeamento EF incompleto (VOs Money/EmvCode/ChargeId sem conversor).
    // Os DbSets sao mantidos para os repositorios compilarem, mas os tipos sao
    // ignorados no modelo (Ignore<>() em OnModelCreating) ate serem persistidos.
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PixCharge> PixCharges => Set<PixCharge>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(c => c.Id);

        // Mapeia o agregado Credential com o id fortemente tipado (CredentialId)
        modelBuilder.Entity<Credential>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Id)
                .HasConversion(id => id.Value, value => CredentialId.From(value));
            b.Ignore(s => s.DomainEvents); // colecao de eventos nao e persistida
        });

        // Relacionamento entre Conta e Credential tratado via CredentialId (Foreign Key)
        modelBuilder.Entity<Account>()
            .Property(c => c.CredentialId) // Alterado de SecretId para CredentialId
            .IsRequired(false);

        // A navegacao Conta -> Credential nao e um relacionamento mapeado:
        // CredentialId e int legado, enquanto o agregado usa CredentialId/Guid.
        modelBuilder.Entity<Account>().Ignore(c => c.Credential);
        
        modelBuilder.Entity<PixKey>()
            .HasOne(cp => cp.Account)
            .WithMany()
            .HasForeignKey(cp => cp.AccountId);

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
            b.ToTable("cobrancas");
        });

        // Invoice/PixCharge ficam fora do modelo EF por enquanto (mapeamento incompleto).
        // Reativar: remover os Ignore<>() e aplicar as configuracoes abaixo.
        modelBuilder.Ignore<Invoice>();
        modelBuilder.Ignore<PixCharge>();
        // modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        // modelBuilder.ApplyConfiguration(new PixChargeConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}