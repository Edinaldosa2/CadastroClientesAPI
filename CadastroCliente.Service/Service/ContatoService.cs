using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Aplicacao.Mapping;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;
using FluentValidation;

namespace CadastroCliente.Service.Service;

public sealed class ContatoService : IContatoAppService
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IValidator<ContatoRequest> _validator;

    public ContatoService(
        IClienteRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IValidator<ContatoRequest> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ContatoDto>> ListarAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        return cliente.Contatos.Select(ClienteMapper.ToDto).ToList();
    }

    public async Task<ContatoDto> ObterAsync(Guid clienteId, Guid contatoId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        return ClienteMapper.ToDto(cliente.ObterContato(contatoId));
    }

    public async Task<ContatoDto> CriarAsync(Guid clienteId, ContatoRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var cliente = await ObterCliente(clienteId, cancellationToken);
        var contato = cliente.AdicionarContato(request.Tipo, request.Valor, request.Nome, request.Principal, _clock.UtcNow);
        _unitOfWork.RegisterNew(contato);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(contato);
    }

    public async Task<ContatoDto> AtualizarAsync(Guid clienteId, Guid contatoId, ContatoRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var cliente = await ObterCliente(clienteId, cancellationToken);
        var contato = cliente.AtualizarContato(contatoId, request.Tipo, request.Valor, request.Nome, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(contato);
    }

    public async Task RemoverAsync(Guid clienteId, Guid contatoId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        cliente.RemoverContato(contatoId, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cliente> ObterCliente(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, false, cancellationToken)
           ?? throw new NotFoundException("Cliente", id);
}
