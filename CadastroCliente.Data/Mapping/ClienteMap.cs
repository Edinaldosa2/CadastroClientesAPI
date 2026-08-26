using CadastroCliente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCliente.Data.Mapping;

public sealed class ClienteMap : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.NomeFantasia).HasMaxLength(150);
        builder.Property(x => x.TipoPessoa).HasConversion<int>().IsRequired();
        builder.Property(x => x.Documento).HasMaxLength(14).IsRequired();
        builder.Property(x => x.InscricaoEstadual).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Observacoes).HasMaxLength(1000);
        builder.Property(x => x.MotivoStatus).HasMaxLength(250);
        builder.Property(x => x.CriadoEm).IsRequired();
        builder.Property(x => x.Excluido).IsRequired();

        builder.HasIndex(x => x.Documento).IsUnique();
        builder.HasIndex(x => x.Nome);
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Enderecos)
            .WithOne()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Contatos)
            .WithOne()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Enderecos).HasField("_enderecos").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Contatos).HasField("_contatos").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(x => !x.Excluido);
    }
}
