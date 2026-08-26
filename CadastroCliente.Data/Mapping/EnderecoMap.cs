using CadastroCliente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCliente.Data.Mapping;

public sealed class EnderecoMap : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("enderecos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Logradouro).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Complemento).HasMaxLength(80);
        builder.Property(x => x.Bairro).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Cidade).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Uf).HasMaxLength(2).IsRequired();
        builder.Property(x => x.Cep).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Pais).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Principal).IsRequired();
        builder.Property(x => x.ClienteId).IsRequired();

        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => new { x.Cidade, x.Uf });
    }
}
