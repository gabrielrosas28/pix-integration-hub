// Path: Infrastructure/Data/Context/Configurations/PixChargeConfiguration.cs
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.PixCharge;
using Domain.ValueObjects;

namespace Infrastructure.Data.Context.Configurations;

public sealed class PixChargeConfiguration : IEntityTypeConfiguration<PixCharge>
{
    public void Configure(EntityTypeBuilder<PixCharge> builder)
    {
        // Define a tabela no banco
        builder.ToTable("PixCharges");

        // Define a chave primária usando o TxId (visto que cada cobrança Pix possui um TxId único)
        builder.HasKey(x => x.TxId);
        builder.Property(x => x.TxId)
            .HasConversion(
                txId => txId.Value,
                value => TxId.From(value))
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Mapeia o Raw (Payload Bruto) como uma coluna JSON nativa do PostgreSQL (jsonb)
        builder.Property(x => x.Raw)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}