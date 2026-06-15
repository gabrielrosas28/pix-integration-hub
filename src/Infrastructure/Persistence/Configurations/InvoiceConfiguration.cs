// Path: Infrastructure/Data/Context/Configurations/InvoiceConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Aggregates.Invoice;
using Domain.ValueObjects;

namespace Infrastructure.Data.Context.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // Define a tabela no banco
        builder.ToTable("Invoices");

        // Chave Primária mapeando o ID Tipado (InvoiceId)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => InvoiceId.From(value))
            .IsRequired();

        // Mapeamento do TxId (Value Object de amarração universal com o Pix)
        builder.Property(x => x.TxId)
            .HasConversion(
                txId => txId.Value,
                value => TxId.From(value))
            .HasMaxLength(35) // Tamanho máximo padrão exigido pelo BACEN
            .IsRequired();

        // Configuração de propriedades comuns
        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>() // Salva o Enum como texto (ex: "Active", "Paid") no banco
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Cria um índice no TxId para buscas rápidas durante a conciliação
        // Usa string para evitar problemas com nullable em expressões lambda
        builder.HasIndex("TxId")
            .IsUnique()
            .HasFilter("\"TxId\" IS NOT NULL");
    }
}