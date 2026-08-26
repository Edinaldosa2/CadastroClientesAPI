using CadastroCliente.Data.Context;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Data.Seed;

public static class ClienteSeed
{
    public static async Task EnsureSeedAsync(CadastroClienteContext context, CancellationToken cancellationToken = default)
    {
        if (context.Clientes.Any())
        {
            return;
        }

        var agora = DateTimeOffset.UtcNow;

        var maria = Cliente.Criar(
            "Maria Silva Santos",
            TipoPessoa.Fisica,
            "52998224725",
            null,
            null,
            new DateTime(1988, 4, 12),
            "Cliente de demonstração — pessoa física.",
            agora);
        maria.AdicionarEndereco(
            TipoEndereco.Residencial,
            "Rua das Flores",
            "100",
            "Apto 12",
            "Centro",
            "São Paulo",
            "SP",
            "01310100",
            "Brasil",
            true,
            agora);
        maria.AdicionarContato(TipoContato.Email, "maria.silva@example.com", "Pessoal", true, agora);
        maria.AdicionarContato(TipoContato.Celular, "11987654321", "Celular", true, agora);

        var acme = Cliente.Criar(
            "Acme Comércio Ltda",
            TipoPessoa.Juridica,
            "11222333000181",
            "Acme Store",
            "172.16.1.3",
            new DateTime(2010, 1, 15),
            "Cliente de demonstração — pessoa jurídica.",
            agora);
        acme.AdicionarEndereco(
            TipoEndereco.Comercial,
            "Avenida Paulista",
            "1578",
            "Conjunto 101",
            "Bela Vista",
            "São Paulo",
            "SP",
            "01310200",
            "Brasil",
            true,
            agora);
        acme.AdicionarEndereco(
            TipoEndereco.Entrega,
            "Rua Vergueiro",
            "2000",
            null,
            "Vila Mariana",
            "São Paulo",
            "SP",
            "04101000",
            "Brasil",
            false,
            agora);
        acme.AdicionarContato(TipoContato.Email, "contato@acme.example.com", "Comercial", true, agora);
        acme.AdicionarContato(TipoContato.Telefone, "1133334444", "Recepção", true, agora);

        await context.Clientes.AddRangeAsync(new[] { maria, acme }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
