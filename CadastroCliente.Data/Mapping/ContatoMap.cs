using CadastroCliente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCliente.Data.Mapping;

public sealed class ContatoMap : IEntityTypeConfiguration<Contato>
{
    public void Configure(EntityTypeBuilder<Contato> builder)
    {
        builder.ToTable("contatos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Valor).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(80);
        builder.Property(x => x.Principal).IsRequired();
        builder.Property(x => x.ClienteId).IsRequired();

        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => new { x.ClienteId, x.Tipo, x.Valor }).IsUnique();
    }
}
