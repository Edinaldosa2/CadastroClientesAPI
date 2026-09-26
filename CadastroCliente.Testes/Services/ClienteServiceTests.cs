using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using CadastroCliente.Service.Service;
using FluentAssertions;
using Moq;

namespace CadastroCliente.Testes.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IClock> _clock = new();
    private readonly ClienteService _service;

    public ClienteServiceTests()
    {
        _clock.Setup(x => x.UtcNow).Returns(new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero));
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _service = new ClienteService(_repo.Object, _uow.Object, _clock.Object, new CriarClienteRequestValidator(), new AtualizarClienteRequestValidator());
    }

    [Fact]
    public async Task Criar_persiste_cliente()
    {
        _repo.Setup(x => x.ExistsDocumentoAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var dto = await _service.CriarAsync(new CriarClienteRequest
        {
            Nome = "Maria Silva",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "52998224725",
            Enderecos =
            {
                new EnderecoRequest
                {
                    Logradouro = "Rua das Flores",
                    Numero = "10",
                    Bairro = "Centro",
                    Cidade = "São Paulo",
                    Uf = "SP",
                    Cep = "01310100",
                    Principal = true
                }
            },
            Contatos = { new ContatoRequest { Tipo = TipoContato.Email, Valor = "maria@example.com" } }
        });

        dto.Nome.Should().Be("Maria Silva");
        dto.Enderecos.Should().ContainSingle();
        _repo.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_documento_duplicado()
    {
        _repo.Setup(x => x.ExistsDocumentoAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var act = () => _service.CriarAsync(new CriarClienteRequest
        {
            Nome = "Maria Silva",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "52998224725"
        });
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Obter_nao_encontrado()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var act = () => _service.ObterPorIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Listar_mapeia_pagina()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, _clock.Object.UtcNow);
        _repo.Setup(x => x.SearchAsync(It.IsAny<ClienteFiltro>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Cliente>(new[] { cliente }, 1, 20, 1));
        var pagina = await _service.ListarAsync(new ClienteFiltro());
        pagina.Total.Should().Be(1);
        pagina.Itens[0].Nome.Should().Be("Maria Silva");
    }

    [Fact]
    public async Task Atualizar_e_status()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, _clock.Object.UtcNow);
        _repo.Setup(x => x.GetByIdAsync(cliente.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var atualizado = await _service.AtualizarAsync(cliente.Id, new AtualizarClienteRequest { Nome = "Maria Souza" });
        atualizado.Nome.Should().Be("Maria Souza");

        await _service.InativarAsync(cliente.Id, new AlterarStatusRequest { Motivo = "teste" });
        cliente.Status.Should().Be(StatusCliente.Inativo);
        await _service.AtivarAsync(cliente.Id);
        await _service.BloquearAsync(cliente.Id, new AlterarStatusRequest { Motivo = "risco" });
        cliente.Status.Should().Be(StatusCliente.Bloqueado);
        var desbloqueio = () => _service.AtivarAsync(cliente.Id);
        await desbloqueio.Should().ThrowAsync<BusinessRuleException>().Where(e => e.Code == "motivo_obrigatorio");
        await _service.AtivarAsync(cliente.Id, new AlterarStatusRequest { Motivo = "revisão" });
        cliente.Status.Should().Be(StatusCliente.Ativo);
    }

    [Fact]
    public async Task Excluir_e_restaurar()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, null, _clock.Object.UtcNow);
        _repo.Setup(x => x.GetByIdAsync(cliente.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repo.Setup(x => x.GetByIdAsync(cliente.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repo.Setup(x => x.ExistsDocumentoAtivoAsync(cliente.Documento, cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await _service.ExcluirAsync(cliente.Id);
        cliente.Excluido.Should().BeTrue();
        await _service.RestaurarAsync(cliente.Id);
        cliente.Excluido.Should().BeFalse();

        cliente.Excluir(_clock.Object.UtcNow);
        _repo.Setup(x => x.ExistsDocumentoAtivoAsync(cliente.Documento, cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var conflito = () => _service.RestaurarAsync(cliente.Id);
        await conflito.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Patch_e_obter_por_documento()
    {
        var cliente = Cliente.Criar("Maria Silva", TipoPessoa.Fisica, "52998224725", null, null, null, "old", _clock.Object.UtcNow);
        _repo.Setup(x => x.GetByIdAsync(cliente.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repo.Setup(x => x.GetByDocumentoAsync("52998224725", It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var patched = await _service.PatchAsync(cliente.Id, new PatchClienteRequest { Nome = "Maria P.", AtualizarObservacoes = true, Observacoes = "x" });
        patched.Nome.Should().Be("Maria P.");
        patched.Observacoes.Should().Be("x");
        (await _service.ObterPorDocumentoAsync("52998224725")).Id.Should().Be(cliente.Id);

        var documento = () => _service.PatchAsync(cliente.Id, new PatchClienteRequest { Documento = "11144477735" });
        await documento.Should().ThrowAsync<BusinessRuleException>().Where(e => e.Code == "documento_imutavel");
    }
}
