using CadastroCliente.Aplicacao.Mapping;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using FluentAssertions;

namespace CadastroCliente.Testes.Domain;

public class MapperAndExceptionTests
{
    [Fact]
    public void Mapper_projeta_list_item_e_dto()
    {
        var agora = DateTimeOffset.UtcNow;
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, "obs", agora);
        cliente.AdicionarEndereco(TipoEndereco.Residencial, "Rua das Flores", "10", null, "Centro", "São Paulo", "SP", "01310100", null, true, agora);
        cliente.AdicionarContato(TipoContato.Email, "maria@example.com", null, true, agora);

        var dto = ClienteMapper.ToDto(cliente);
        dto.Enderecos.Should().ContainSingle();
        dto.Contatos.Should().ContainSingle();
        dto.DocumentoFormatado.Should().Contain(".");

        var item = ClienteMapper.ToListItem(cliente);
        item.Cidade.Should().Be("São Paulo");
        item.Uf.Should().Be("SP");
    }

    [Fact]
    public void BusinessRule_com_dicionario()
    {
        var ex = new BusinessRuleException(new Dictionary<string, string[]> { ["nome"] = new[] { "obrigatório" } });
        ex.Errors["nome"].Should().Contain("obrigatório");
        ex.Code.Should().Be("validation");
        new ConflictException("x").Code.Should().Be("conflict");
        new NotFoundException("Cliente", 1).Message.Should().Contain("Cliente");
    }
}
