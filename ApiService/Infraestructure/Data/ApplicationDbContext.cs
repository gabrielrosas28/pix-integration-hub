using Microsoft.EntityFrameworkCore;
using ApiService.Domain.Entities;

namespace ApiService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conta> Contas { get; set; }
    public DbSet<Secret> Secrets { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conta>()
            .HasKey(c => c.ClientId);

        modelBuilder.Entity<Conta>()
            .HasOne(c => c.Secret)
            .WithOne(s => s.Conta)
            .HasForeignKey<Secret>(s => s.ClientId);

        base.OnModelCreating(modelBuilder);
    }
}