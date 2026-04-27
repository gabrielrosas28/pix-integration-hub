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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conta>().HasKey(c => c.ClientId);

        modelBuilder.Entity<Conta>()
            .HasOne(c => c.Secret)
            .WithOne(s => s.Conta)
            .HasForeignKey<Secret>(s => s.ClientId);

        base.OnModelCreating(modelBuilder);
    }
}