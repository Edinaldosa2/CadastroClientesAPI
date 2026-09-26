using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Events;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using CadastroCliente.Service.Events;
using CadastroCliente.Service.Service;
using FluentAssertions;
using Moq;

namespace CadastroCliente.Testes.Services;

public class AuditoriaServiceTests
{
    [Fact]
    public async Task Listar_exige_cliente_existente()
    {
        var clientes = new Mock<IClienteRepository>();
        var auditoria = new Mock<IAuditoriaRepository>();
        clientes.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var service = new AuditoriaService(clientes.Object, auditoria.Object);

        var act = () => service.ListarPorClienteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handler_persiste_evento_de_criacao()
    {
        var repo = new Mock<IAuditoriaRepository>();
        AuditoriaItem? gravado = null;
        repo.Setup(x => x.AddAsync(It.IsAny<AuditoriaItem>(), It.IsAny<CancellationToken>()))
            .Callback<AuditoriaItem, CancellationToken>((item, _) => gravado = item)
            .Returns(Task.CompletedTask);

        var handler = new PersistirAuditoriaHandler(repo.Object, new UsuarioFixo("editor"), new CorrelacaoFixa("trace-1"));
        await handler.HandleAsync(new ClienteCriado(Guid.NewGuid(), "Maria Silva", "52998224725", DateTimeOffset.UtcNow));

        gravado.Should().NotBeNull();
        gravado!.Tipo.Should().Be("cliente.criado");
        gravado.Usuario.Should().Be("editor");
        gravado.CorrelationId.Should().Be("trace-1");
    }

    private sealed class UsuarioFixo : ICurrentUser
    {
        public UsuarioFixo(string usuario) => Usuario = usuario;
        public bool Autenticado => true;
        public string? Usuario { get; }
    }

    private sealed class CorrelacaoFixa : ICorrelationContext
    {
        public CorrelacaoFixa(string id) => CorrelationId = id;
        public string? CorrelationId { get; }
    }
}
