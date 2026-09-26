using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Aplicacao.Mapping;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Domain.Query;
using FluentValidation;

namespace CadastroCliente.Service.Service;

public sealed class ClienteService : IClienteAppService
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IValidator<CriarClienteRequest> _criarValidator;
    private readonly IValidator<AtualizarClienteRequest> _atualizarValidator;

    public ClienteService(
        IClienteRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IValidator<CriarClienteRequest> criarValidator,
        IValidator<AtualizarClienteRequest> atualizarValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _criarValidator = criarValidator;
        _atualizarValidator = atualizarValidator;
    }

    public async Task<PagedResult<ClienteListItemDto>> ListarAsync(ClienteFiltro filtro, CancellationToken cancellationToken = default)
    {
        var pagina = await _repository.SearchAsync(filtro, cancellationToken);
        var itens = pagina.Itens.Select(ClienteMapper.ToListItem).ToList();
        return new PagedResult<ClienteListItemDto>(itens, pagina.Pagina, pagina.TamanhoPagina, pagina.Total, pagina.ProximoCursor);
    }

    public async Task<ClienteDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(id, cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.GetByDocumentoAsync(documento, cancellationToken)
                      ?? throw new NotFoundException("Cliente", documento);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default)
    {
        await _criarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _repository.ExistsDocumentoAsync(request.Documento, null, cancellationToken))
        {
            throw new ConflictException("Já existe um cliente com este documento.");
        }

        var agora = _clock.UtcNow;
        var cliente = Cliente.Criar(
            request.Nome,
            request.TipoPessoa,
            request.Documento,
            request.NomeFantasia,
            request.InscricaoEstadual,
            request.DataNascimento,
            request.Observacoes,
            agora);

        foreach (var endereco in request.Enderecos)
        {
            cliente.AdicionarEndereco(
                endereco.Tipo,
                endereco.Logradouro,
                endereco.Numero,
                endereco.Complemento,
                endereco.Bairro,
                endereco.Cidade,
                endereco.Uf,
                endereco.Cep,
                endereco.Pais,
                endereco.Principal,
                agora);
        }

        foreach (var contato in request.Contatos)
        {
            cliente.AdicionarContato(contato.Tipo, contato.Valor, contato.Nome, contato.Principal, agora);
        }

        await _repository.AddAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(Guid id, AtualizarClienteRequest request, CancellationToken cancellationToken = default)
    {
        await _atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var cliente = await ObterCliente(id, cancellationToken);
        cliente.Atualizar(request.Nome, request.NomeFantasia, request.InscricaoEstadual, request.DataNascimento, request.Observacoes, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> PatchAsync(Guid id, PatchClienteRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Documento)
            || (request.Extra?.Keys.Any(k => k.Equals("documento", StringComparison.OrdinalIgnoreCase)) ?? false))
        {
            throw new BusinessRuleException("documento_imutavel", "documento", "Documento não pode ser alterado.");
        }

        var cliente = await ObterCliente(id, cancellationToken);
        cliente.AtualizarParcial(
            request.Nome,
            request.NomeFantasia,
            request.InscricaoEstadual,
            request.DataNascimento,
            request.AtualizarDataNascimento,
            request.Observacoes,
            request.AtualizarObservacoes,
            _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(id, cancellationToken);
        cliente.Excluir(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ClienteDto> RestaurarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.GetByIdAsync(id, incluirExcluidos: true, cancellationToken)
                      ?? throw new NotFoundException("Cliente", id);
        if (await _repository.ExistsDocumentoAtivoAsync(cliente.Documento, cliente.Id, cancellationToken))
        {
            throw new ConflictException("Já existe um cliente ativo com este documento. Restore bloqueado.");
        }

        cliente.Restaurar(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> AtivarAsync(Guid id, AlterarStatusRequest? request = null, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(id, cancellationToken);
        cliente.Ativar(_clock.UtcNow, request?.Motivo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> InativarAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(id, cancellationToken);
        cliente.Inativar(request.Motivo, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    public async Task<ClienteDto> BloquearAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterCliente(id, cancellationToken);
        cliente.Bloquear(request.Motivo, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToDto(cliente);
    }

    private async Task<Cliente> ObterCliente(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, false, cancellationToken)
           ?? throw new NotFoundException("Cliente", id);
}
