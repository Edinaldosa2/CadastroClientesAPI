using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Query;
using FluentAssertions;

namespace CadastroCliente.Testes.Domain;

public class ClienteTests
{
    private static readonly DateTimeOffset Agora = new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Criar_pessoa_fisica()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", "Ignorado", null, new DateTime(1990, 1, 1), "obs", Agora);
        cliente.Nome.Should().Be("Maria Silva");
        cliente.NomeFantasia.Should().BeNull();
        cliente.Status.Should().Be(StatusCliente.Ativo);
        cliente.Documento.Should().Be("52998224725");
        cliente.DataNascimento.Should().Be(new DateTime(1990, 1, 1));
    }

    [Fact]
    public void Criar_pessoa_juridica()
    {
        var cliente = Cliente.Criar("Acme Ltda", TipoPessoa.Juridica, "11222333000181", "Acme", "123", new DateTime(2010, 1, 1), null, Agora);
        cliente.NomeFantasia.Should().Be("Acme");
        cliente.TipoPessoa.Should().Be(TipoPessoa.Juridica);
    }

    [Fact]
    public void Nome_curto_e_invalido()
    {
        var act = () => Cliente.Criar("Al", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Nascimento_futuro_invalido()
    {
        var act = () => Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, Agora.UtcDateTime.Date.AddDays(1), null, Agora);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Ciclo_de_status()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        cliente.Inativar("férias", Agora);
        cliente.Status.Should().Be(StatusCliente.Inativo);
        cliente.Ativar(Agora);
        cliente.Bloquear("risco", Agora);
        cliente.Status.Should().Be(StatusCliente.Bloqueado);
        var editar = () => cliente.Atualizar("Outro Nome", null, null, null, null, Agora);
        editar.Should().Throw<BusinessRuleException>();
        cliente.Ativar(Agora);
        cliente.Status.Should().Be(StatusCliente.Ativo);
    }

    [Fact]
    public void Status_duplicado_falha()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        var act = () => cliente.Ativar(Agora);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Excluir_e_restaurar()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        cliente.Excluir(Agora);
        cliente.Excluido.Should().BeTrue();
        var alterar = () => cliente.Inativar(null, Agora);
        alterar.Should().Throw<BusinessRuleException>();
        cliente.Restaurar(Agora);
        cliente.Excluido.Should().BeFalse();
        cliente.Status.Should().Be(StatusCliente.Ativo);
    }

    [Fact]
    public void Enderecos_principal_e_remocao()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        var primeiro = cliente.AdicionarEndereco(TipoEndereco.Residencial, "Rua das Flores", "10", null, "Centro", "São Paulo", "sp", "01310100", null, false, Agora);
        primeiro.Principal.Should().BeTrue();
        var segundo = cliente.AdicionarEndereco(TipoEndereco.Comercial, "Av Paulista", "1000", "cj 1", "Bela Vista", "São Paulo", "SP", "01310200", "Brasil", true, Agora);
        segundo.Principal.Should().BeTrue();
        cliente.Enderecos.Single(e => e.Id == primeiro.Id).Principal.Should().BeFalse();
        cliente.DefinirEnderecoPrincipal(primeiro.Id, Agora);
        primeiro.Principal.Should().BeTrue();
        cliente.RemoverEndereco(primeiro.Id, Agora);
        cliente.Enderecos.Should().ContainSingle().Which.Principal.Should().BeTrue();
        var inexistente = () => cliente.ObterEndereco(Guid.NewGuid());
        inexistente.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Uf_invalida()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        var act = () => cliente.AdicionarEndereco(TipoEndereco.Residencial, "Rua das Flores", "10", null, "Centro", "Lisboa", "XX", "01310100", null, true, Agora);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Contatos_duplicados_e_principal_por_tipo()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, Agora);
        var email = cliente.AdicionarContato(TipoContato.Email, "maria@example.com", "Pessoal", true, Agora);
        email.Principal.Should().BeTrue();
        var dup = () => cliente.AdicionarContato(TipoContato.Email, "MARIA@example.com", null, false, Agora);
        dup.Should().Throw<ConflictException>();
        var celular = cliente.AdicionarContato(TipoContato.Celular, "(11) 98765-4321", null, true, Agora);
        celular.Valor.Should().Be("11987654321");
        cliente.RemoverContato(email.Id, Agora);
        cliente.Contatos.Should().ContainSingle(c => c.Id == celular.Id);
    }

    [Fact]
    public void Patch_parcial()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, new DateTime(1990, 1, 1), "old", Agora);
        cliente.AtualizarParcial("Maria Souza", null, null, null, false, "nova obs", true, Agora);
        cliente.Nome.Should().Be("Maria Souza");
        cliente.DataNascimento.Should().Be(new DateTime(1990, 1, 1));
        cliente.Observacoes.Should().Be("nova obs");
    }

    [Fact]
    public void Filtro_normaliza_paginacao()
    {
        var filtro = new ClienteFiltro { Pagina = 0, TamanhoPagina = 500 };
        filtro.PaginaNormalizada.Should().Be(1);
        filtro.TamanhoNormalizado.Should().Be(100);
        var page = new PagedResult<int>(new[] { 1, 2 }, 1, 2, 5);
        page.TotalPaginas.Should().Be(3);
        page.TemProxima.Should().BeTrue();
        page.TemAnterior.Should().BeFalse();
    }
}
