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
        modelBuilder.Entity<Conta>().HasKey(c => c.Id);

        // configure Secret primary key and FK relationship via SecretId in Conta
        modelBuilder.Entity<Secret>().HasKey(s => s.SecretID);

        modelBuilder.Entity<Conta>()
            .HasOne(c => c.Secret)
            .WithMany()
            .HasForeignKey(c => c.SecretId)
            .HasPrincipalKey<Secret>(s => s.SecretID);

        base.OnModelCreating(modelBuilder);
    }
}