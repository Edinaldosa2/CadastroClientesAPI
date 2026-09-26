using CadastroCliente.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCliente.Data.Mapping;

public sealed class IdempotencyMap : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(80);
        builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(80);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
