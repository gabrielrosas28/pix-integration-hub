using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Secret> Secrets => Set<Secret>();
    public DbSet<PixKey> PixKeys => Set<PixKey>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Charge> Charges => Set<Charge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(c => c.Id);
            b.ToTable("Contas");
            b.Property(c => c.Document).HasColumnName("Documento");
            b.Property(c => c.AccountNumber).HasColumnName("NumeroConta");
            b.Property(c => c.Agency).HasColumnName("Agencia");
            b.HasOne(c => c.Secret)
             .WithMany()
             .HasForeignKey(c => c.SecretId);
        });

        modelBuilder.Entity<Secret>(b =>
        {
            b.HasKey(s => s.Id);
            b.ToTable("Secrets");
            b.Property(s => s.ClientSecret).HasColumnName("ClienteSecret");
            b.Property(s => s.Certificate).HasColumnName("Certificado");
            b.Property(s => s.CertificatePassword).HasColumnName("SenhaCertificado");
        });

        modelBuilder.Entity<PixKey>(b =>
        {
            b.HasKey(p => p.Id);
            b.ToTable("ChavesPix");
            b.Property(p => p.Key).HasColumnName("Chave");
            b.Property(p => p.Type).HasColumnName("Tipo");
            b.Property(p => p.AccountId).HasColumnName("ContaId");
            b.HasOne(p => p.Account)
             .WithMany()
             .HasForeignKey(p => p.AccountId);
        });

        modelBuilder.Entity<Charge>(b =>
        {
            b.HasKey(c => c.Id);
            b.ToTable("cobrancas");
            b.Property(c => c.TxId).IsRequired();
            b.Property(c => c.InvoiceId).HasColumnName("InvoiceID");
            b.Property(c => c.Amount).HasColumnType("numeric");
        });

        modelBuilder.Entity<Audit>(b =>
        {
            b.HasKey(a => a.Id);
            b.HasKey("AuditoriaId");
            b.ToTable("Auditorias");
            b.Property(a => a.AccountId).HasColumnName("ContaId");
            b.Property(a => a.RegisteredAt).HasColumnName("HorarioRegistro");
            b.Property(a => a.PaymentStatus).HasColumnName("StatusPagamento");
            b.Property(a => a.TxId).HasColumnName("TxId").HasColumnType("text");
            b.Property(a => a.Description).HasColumnName("Descricao");
            b.Property(a => a.Amount).HasColumnName("Valor");
            b.Property(a => a.ActionPayload).HasColumnName("PayloadAcao");
            b.Property(a => a.ConfirmationPayload).HasColumnName("PayloadConfirmacao");
            b.Property(a => a.PaymentDate).HasColumnName("DataPagamento");
            b.Property(a => a.ConfirmationTime).HasColumnName("HorarioConfirmacao");
        });
    }
}

/// <summary>
/// Unit of Work implementation backed by ApplicationDbContext.
/// </summary>
public sealed class EfUnitOfWork : Application.Interfaces.IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public EfUnitOfWork(ApplicationDbContext context) => _context = context;

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
