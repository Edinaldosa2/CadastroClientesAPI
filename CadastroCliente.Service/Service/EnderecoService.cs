using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Aplicacao.Mapping;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;
using FluentValidation;

namespace CadastroCliente.Service.Service;

public sealed class EnderecoService : IEnderecoAppService
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IValidator<EnderecoRequest> _validator;

    public EnderecoService(
        IClienteRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IValidator<EnderecoRequest> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _validator = validator;
    }

    public async Task<IReadOnlyList<EnderecoDto>> ListarAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        return cliente.Enderecos.Select(ClienteMapper.ToDto).ToList();
    }

    public async Task<EnderecoDto> ObterAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        return ClienteMapper.ToDto(cliente.ObterEndereco(enderecoId));
    }

    public async Task<EnderecoDto> CriarAsync(Guid clienteId, EnderecoRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var cliente = await ObterCliente(clienteId, cancellationToken);
        var endereco = cliente.AdicionarEndereco(
            request.Tipo,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Cep,
            request.Pais,
            request.Principal,
            _clock.UtcNow);
        _unitOfWork.RegisterNew(endereco);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(endereco);
    }

    public async Task<EnderecoDto> AtualizarAsync(Guid clienteId, Guid enderecoId, EnderecoRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var cliente = await ObterCliente(clienteId, cancellationToken);
        var endereco = cliente.AtualizarEndereco(
            enderecoId,
            request.Tipo,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Cep,
            request.Pais,
            _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(endereco);
    }

    public async Task<EnderecoDto> DefinirPrincipalAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        cliente.DefinirEnderecoPrincipal(enderecoId, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente.ObterEndereco(enderecoId));
    }

    public async Task RemoverAsync(Guid clienteId, Guid enderecoId, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(clienteId, cancellationToken);
        cliente.RemoverEndereco(enderecoId, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cliente> ObterCliente(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, false, cancellationToken)
           ?? throw new NotFoundException("Cliente", id);
}
