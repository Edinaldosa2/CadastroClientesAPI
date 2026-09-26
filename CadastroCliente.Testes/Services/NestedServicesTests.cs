using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using CadastroCliente.Service.Service;
using FluentAssertions;
using Moq;

namespace CadastroCliente.Testes.Services;

public class NestedServicesTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Cliente _cliente;
    private readonly EnderecoService _enderecos;
    private readonly ContatoService _contatos;
    private readonly RelatorioService _relatorios;

    public NestedServicesTests()
    {
        _clock.Setup(x => x.UtcNow).Returns(new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero));
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _uow.Setup(x => x.RegisterNew(It.IsAny<Endereco>()));
        _uow.Setup(x => x.RegisterNew(It.IsAny<Contato>()));
        _cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, _clock.Object.UtcNow);
        _repo.Setup(x => x.GetByIdAsync(_cliente.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(_cliente);
        _enderecos = new EnderecoService(_repo.Object, _uow.Object, _clock.Object, new EnderecoRequestValidator());
        _contatos = new ContatoService(_repo.Object, _uow.Object, _clock.Object, new ContatoRequestValidator());
        _relatorios = new RelatorioService(_repo.Object);
    }

    [Fact]
    public async Task Endereco_crud()
    {
        var criado = await _enderecos.CriarAsync(_cliente.Id, new EnderecoRequest
        {
            Tipo = TipoEndereco.Residencial,
            Logradouro = "Rua das Flores",
            Numero = "10",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "01310100",
            Principal = true
        });
        criado.Cidade.Should().Be("São Paulo");

        var lista = await _enderecos.ListarAsync(_cliente.Id);
        lista.Should().ContainSingle();
        (await _enderecos.ObterAsync(_cliente.Id, criado.Id)).Id.Should().Be(criado.Id);

        var atualizado = await _enderecos.AtualizarAsync(_cliente.Id, criado.Id, new EnderecoRequest
        {
            Tipo = TipoEndereco.Comercial,
            Logradouro = "Avenida Paulista",
            Numero = "1000",
            Bairro = "Bela Vista",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "01310200"
        });
        atualizado.Logradouro.Should().Be("Avenida Paulista");

        var segundo = await _enderecos.CriarAsync(_cliente.Id, new EnderecoRequest
        {
            Tipo = TipoEndereco.Entrega,
            Logradouro = "Rua Vergueiro",
            Numero = "50",
            Bairro = "Liberdade",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "01504001",
            Principal = false
        });
        await _enderecos.DefinirPrincipalAsync(_cliente.Id, segundo.Id);
        (await _enderecos.ObterAsync(_cliente.Id, segundo.Id)).Principal.Should().BeTrue();
        await _enderecos.RemoverAsync(_cliente.Id, segundo.Id);
        (await _enderecos.ListarAsync(_cliente.Id)).Should().ContainSingle();
    }

    [Fact]
    public async Task Contato_crud()
    {
        var criado = await _contatos.CriarAsync(_cliente.Id, new ContatoRequest
        {
            Tipo = TipoContato.Email,
            Valor = "maria@example.com",
            Nome = "Pessoal"
        });
        criado.Valor.Should().Be("maria@example.com");
        await _contatos.AtualizarAsync(_cliente.Id, criado.Id, new ContatoRequest
        {
            Tipo = TipoContato.Email,
            Valor = "maria.silva@example.com"
        });
        (await _contatos.ObterAsync(_cliente.Id, criado.Id)).Valor.Should().Be("maria.silva@example.com");
        (await _contatos.ListarAsync(_cliente.Id)).Should().HaveCount(1);
        await _contatos.RemoverAsync(_cliente.Id, criado.Id);
        (await _contatos.ListarAsync(_cliente.Id)).Should().BeEmpty();
    }

    [Fact]
    public async Task Relatorio_resumo_e_csv()
    {
        _repo.Setup(x => x.GetEstatisticasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ClienteEstatisticas
        {
            Total = 2,
            Ativos = 2,
            PessoasFisicas = 1,
            PessoasJuridicas = 1,
            PorUf = new Dictionary<string, long> { ["SP"] = 2 }
        });
        _repo.Setup(x => x.SearchAsync(It.IsAny<ClienteFiltro>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Cliente>(new[] { _cliente }, 1, 100, 1));

        var resumo = await _relatorios.ObterResumoAsync();
        resumo.Total.Should().Be(2);
        var csv = await _relatorios.ExportarCsvAsync();
        csv.Should().StartWith("\uFEFF");
        csv.Should().Contain("Maria Silva");
        csv.Should().Contain("Id;Nome");
    }
}
