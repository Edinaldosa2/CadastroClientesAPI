using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.ValueObjects;

namespace CadastroCliente.Domain.Entities;

public sealed class Cliente : EntityBase
{
    private readonly List<Endereco> _enderecos = new();
    private readonly List<Contato> _contatos = new();

    public string Nome { get; private set; } = string.Empty;
    public string? NomeFantasia { get; private set; }
    public TipoPessoa TipoPessoa { get; private set; }
    public string Documento { get; private set; } = string.Empty;
    public string? InscricaoEstadual { get; private set; }
    public DateTime? DataNascimento { get; private set; }
    public StatusCliente Status { get; private set; }
    public string? Observacoes { get; private set; }
    public string? MotivoStatus { get; private set; }
    public bool Excluido { get; private set; }
    public DateTimeOffset? ExcluidoEm { get; private set; }

    public IReadOnlyCollection<Endereco> Enderecos => _enderecos.AsReadOnly();
    public IReadOnlyCollection<Contato> Contatos => _contatos.AsReadOnly();

    private Cliente()
    {
    }

    public static Cliente Criar(
        string nome,
        TipoPessoa tipoPessoa,
        string documento,
        string? nomeFantasia,
        string? inscricaoEstadual,
        DateTime? dataNascimento,
        string? observacoes,
        DateTimeOffset agora)
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            CriadoEm = agora.UtcDateTime,
            Status = StatusCliente.Ativo
        };

        cliente.AplicarDados(nome, tipoPessoa, documento, nomeFantasia, inscricaoEstadual, dataNascimento, observacoes, agora);
        return cliente;
    }

    public void Atualizar(
        string nome,
        string? nomeFantasia,
        string? inscricaoEstadual,
        DateTime? dataNascimento,
        string? observacoes,
        DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        AplicarDados(nome, TipoPessoa, Documento, nomeFantasia, inscricaoEstadual, dataNascimento, observacoes, agora);
    }

    public void AtualizarParcial(
        string? nome,
        string? nomeFantasia,
        string? inscricaoEstadual,
        DateTime? dataNascimento,
        bool atualizarNascimento,
        string? observacoes,
        bool atualizarObservacoes,
        DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        AplicarDados(
            nome ?? Nome,
            TipoPessoa,
            Documento,
            nomeFantasia ?? NomeFantasia,
            inscricaoEstadual ?? InscricaoEstadual,
            atualizarNascimento ? dataNascimento : DataNascimento,
            atualizarObservacoes ? observacoes : Observacoes,
            agora);
    }

    public void Ativar(DateTimeOffset agora)
    {
        GarantirNaoExcluido();
        if (Status == StatusCliente.Ativo)
        {
            throw new BusinessRuleException("status", "Cliente já está ativo.");
        }

        Status = StatusCliente.Ativo;
        MotivoStatus = null;
        Tocar(agora);
    }

    public void Inativar(string? motivo, DateTimeOffset agora)
    {
        GarantirNaoExcluido();
        if (Status == StatusCliente.Inativo)
        {
            throw new BusinessRuleException("status", "Cliente já está inativo.");
        }

        Status = StatusCliente.Inativo;
        MotivoStatus = NormalizarMotivo(motivo);
        Tocar(agora);
    }

    public void Bloquear(string? motivo, DateTimeOffset agora)
    {
        GarantirNaoExcluido();
        if (Status == StatusCliente.Bloqueado)
        {
            throw new BusinessRuleException("status", "Cliente já está bloqueado.");
        }

        Status = StatusCliente.Bloqueado;
        MotivoStatus = NormalizarMotivo(motivo);
        Tocar(agora);
    }

    public void Excluir(DateTimeOffset agora)
    {
        if (Excluido)
        {
            throw new BusinessRuleException("cliente", "Cliente já foi excluído.");
        }

        Excluido = true;
        ExcluidoEm = agora;
        Status = StatusCliente.Inativo;
        Tocar(agora);
    }

    public void Restaurar(DateTimeOffset agora)
    {
        if (!Excluido)
        {
            throw new BusinessRuleException("cliente", "Cliente não está excluído.");
        }

        Excluido = false;
        ExcluidoEm = null;
        Status = StatusCliente.Ativo;
        Tocar(agora);
    }

    public Endereco AdicionarEndereco(
        TipoEndereco tipo,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        string cep,
        string? pais,
        bool principal,
        DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        if (_enderecos.Count >= 10)
        {
            throw new BusinessRuleException("enderecos", "Limite de 10 endereços por cliente.");
        }

        var deveSerPrincipal = principal || _enderecos.Count == 0;
        if (deveSerPrincipal)
        {
            foreach (var atual in _enderecos.Where(e => e.Principal))
            {
                atual.DesmarcarPrincipal(agora);
            }
        }

        var endereco = new Endereco(
            Id,
            tipo,
            logradouro,
            numero,
            complemento,
            bairro,
            cidade,
            uf,
            Cep.Criar(cep),
            pais,
            deveSerPrincipal,
            agora);

        _enderecos.Add(endereco);
        Tocar(agora);
        return endereco;
    }

    public Endereco AtualizarEndereco(
        Guid enderecoId,
        TipoEndereco tipo,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        string cep,
        string? pais,
        DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        var endereco = ObterEndereco(enderecoId);
        endereco.Atualizar(tipo, logradouro, numero, complemento, bairro, cidade, uf, Cep.Criar(cep), pais, agora);
        Tocar(agora);
        return endereco;
    }

    public void DefinirEnderecoPrincipal(Guid enderecoId, DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        var endereco = ObterEndereco(enderecoId);
        foreach (var atual in _enderecos.Where(e => e.Principal && e.Id != enderecoId))
        {
            atual.DesmarcarPrincipal(agora);
        }

        endereco.MarcarComoPrincipal(agora);
        Tocar(agora);
    }

    public void RemoverEndereco(Guid enderecoId, DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        var endereco = ObterEndereco(enderecoId);
        _enderecos.Remove(endereco);
        if (endereco.Principal && _enderecos.Count > 0)
        {
            _enderecos[0].MarcarComoPrincipal(agora);
        }

        Tocar(agora);
    }

    public Contato AdicionarContato(TipoContato tipo, string valor, string? nome, bool principal, DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        if (_contatos.Count >= 15)
        {
            throw new BusinessRuleException("contatos", "Limite de 15 contatos por cliente.");
        }

        var normalizado = new Contato(Id, tipo, valor, nome, false, agora);
        if (_contatos.Any(c => c.Tipo == normalizado.Tipo && string.Equals(c.Valor, normalizado.Valor, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException("Contato já cadastrado para este cliente.");
        }

        var deveSerPrincipal = principal || !_contatos.Any(c => c.Tipo == tipo);
        if (deveSerPrincipal)
        {
            foreach (var atual in _contatos.Where(c => c.Tipo == tipo && c.Principal))
            {
                atual.DesmarcarPrincipal(agora);
            }
        }

        var contato = new Contato(Id, tipo, valor, nome, deveSerPrincipal, agora);
        _contatos.Add(contato);
        Tocar(agora);
        return contato;
    }

    public Contato AtualizarContato(Guid contatoId, TipoContato tipo, string valor, string? nome, DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        var contato = ObterContato(contatoId);
        var candidato = new Contato(Id, tipo, valor, nome, contato.Principal, agora);
        if (_contatos.Any(c => c.Id != contatoId && c.Tipo == candidato.Tipo && string.Equals(c.Valor, candidato.Valor, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException("Contato já cadastrado para este cliente.");
        }

        contato.Atualizar(tipo, valor, nome, agora);
        Tocar(agora);
        return contato;
    }

    public void RemoverContato(Guid contatoId, DateTimeOffset agora)
    {
        GarantirAtivoParaEdicao();
        var contato = ObterContato(contatoId);
        _contatos.Remove(contato);
        if (contato.Principal)
        {
            var proximo = _contatos.FirstOrDefault(c => c.Tipo == contato.Tipo);
            proximo?.MarcarComoPrincipal(agora);
        }

        Tocar(agora);
    }

    public Endereco ObterEndereco(Guid enderecoId)
        => _enderecos.FirstOrDefault(e => e.Id == enderecoId)
           ?? throw new NotFoundException("Endereco", enderecoId);

    public Contato ObterContato(Guid contatoId)
        => _contatos.FirstOrDefault(c => c.Id == contatoId)
           ?? throw new NotFoundException("Contato", contatoId);

    private void AplicarDados(
        string nome,
        TipoPessoa tipoPessoa,
        string documento,
        string? nomeFantasia,
        string? inscricaoEstadual,
        DateTime? dataNascimento,
        string? observacoes,
        DateTimeOffset agora)
    {
        var doc = ValueObjects.Documento.Criar(documento, tipoPessoa);
        Nome = NormalizarNome(nome);
        TipoPessoa = tipoPessoa;
        Documento = doc.Numero;
        NomeFantasia = tipoPessoa == TipoPessoa.Juridica && !string.IsNullOrWhiteSpace(nomeFantasia)
            ? nomeFantasia.Trim()
            : null;
        InscricaoEstadual = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim();
        DataNascimento = ValidarNascimento(tipoPessoa, dataNascimento, agora);
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        Tocar(agora);
    }

    private static string NormalizarNome(string? nome)
    {
        var valor = nome?.Trim() ?? string.Empty;
        if (valor.Length is < 3 or > 150)
        {
            throw new BusinessRuleException("nome", "Nome deve ter entre 3 e 150 caracteres.");
        }

        return valor;
    }

    private static DateTime? ValidarNascimento(TipoPessoa tipo, DateTime? dataNascimento, DateTimeOffset agora)
    {
        if (tipo == TipoPessoa.Juridica)
        {
            return dataNascimento;
        }

        if (dataNascimento is null)
        {
            return null;
        }

        if (dataNascimento.Value.Date > agora.UtcDateTime.Date)
        {
            throw new BusinessRuleException("dataNascimento", "Data de nascimento não pode ser futura.");
        }

        var idade = agora.UtcDateTime.Year - dataNascimento.Value.Year;
        if (dataNascimento.Value.Date > agora.UtcDateTime.Date.AddYears(-idade))
        {
            idade--;
        }

        if (idade > 120)
        {
            throw new BusinessRuleException("dataNascimento", "Data de nascimento inválida.");
        }

        return dataNascimento.Value.Date;
    }

    private static string? NormalizarMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return null;
        }

        var valor = motivo.Trim();
        return valor.Length > 250 ? valor[..250] : valor;
    }

    private void GarantirAtivoParaEdicao()
    {
        GarantirNaoExcluido();
        if (Status == StatusCliente.Bloqueado)
        {
            throw new BusinessRuleException("status", "Cliente bloqueado não pode ser editado.");
        }
    }

    private void GarantirNaoExcluido()
    {
        if (Excluido)
        {
            throw new BusinessRuleException("cliente", "Cliente excluído não pode ser alterado.");
        }
    }
}
