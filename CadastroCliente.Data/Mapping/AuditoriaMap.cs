using CadastroCliente.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCliente.Data.Mapping;

public sealed class AuditoriaMap : IEntityTypeConfiguration<AuditoriaRecord>
{
    public void Configure(EntityTypeBuilder<AuditoriaRecord> builder)
    {
        builder.ToTable("auditoria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Usuario).HasMaxLength(80);
        builder.Property(x => x.CorrelationId).HasMaxLength(80);
        builder.Property(x => x.OcorridoEm).IsRequired();
        builder.HasIndex(x => new { x.ClienteId, x.OcorridoEm });
    }
}
