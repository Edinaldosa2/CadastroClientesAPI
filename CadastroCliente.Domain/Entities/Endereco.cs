using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.ValueObjects;

namespace CadastroCliente.Domain.Entities;

public sealed class Endereco : EntityBase
{
    public Guid ClienteId { get; private set; }
    public TipoEndereco Tipo { get; private set; }
    public string Logradouro { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Uf { get; private set; } = string.Empty;
    public string Cep { get; private set; } = string.Empty;
    public string Pais { get; private set; } = "Brasil";
    public bool Principal { get; private set; }

    private Endereco()
    {
    }

    internal Endereco(
        Guid clienteId,
        TipoEndereco tipo,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        Cep cep,
        string? pais,
        bool principal,
        DateTimeOffset agora)
    {
        ClienteId = clienteId;
        Tipo = tipo;
        Logradouro = NormalizarTexto(logradouro, "logradouro", 5, 150);
        Numero = NormalizarTexto(numero, "numero", 1, 20);
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = NormalizarTexto(bairro, "bairro", 2, 80);
        Cidade = NormalizarTexto(cidade, "cidade", 2, 80);
        Uf = NormalizarUf(uf);
        Cep = cep.Numero;
        Pais = string.IsNullOrWhiteSpace(pais) ? "Brasil" : pais.Trim();
        Principal = principal;
        CriadoEm = agora.UtcDateTime;
    }

    internal void Atualizar(
        TipoEndereco tipo,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        Cep cep,
        string? pais,
        DateTimeOffset agora)
    {
        Tipo = tipo;
        Logradouro = NormalizarTexto(logradouro, "logradouro", 5, 150);
        Numero = NormalizarTexto(numero, "numero", 1, 20);
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = NormalizarTexto(bairro, "bairro", 2, 80);
        Cidade = NormalizarTexto(cidade, "cidade", 2, 80);
        Uf = NormalizarUf(uf);
        Cep = cep.Numero;
        Pais = string.IsNullOrWhiteSpace(pais) ? "Brasil" : pais.Trim();
        Tocar(agora);
    }

    internal void MarcarComoPrincipal(DateTimeOffset agora)
    {
        Principal = true;
        Tocar(agora);
    }

    internal void DesmarcarPrincipal(DateTimeOffset agora)
    {
        Principal = false;
        Tocar(agora);
    }

    private static string NormalizarTexto(string? valor, string campo, int min, int max)
    {
        var texto = valor?.Trim() ?? string.Empty;
        if (texto.Length < min || texto.Length > max)
        {
            throw new BusinessRuleException(campo, $"{campo} deve ter entre {min} e {max} caracteres.");
        }

        return texto;
    }

    private static string NormalizarUf(string? uf)
    {
        var valor = uf?.Trim().ToUpperInvariant() ?? string.Empty;
        if (valor.Length != 2 || valor.Any(c => c is < 'A' or > 'Z'))
        {
            throw new BusinessRuleException("uf", "UF deve ter 2 letras.");
        }

        string[] ufs =
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
            "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        if (!ufs.Contains(valor))
        {
            throw new BusinessRuleException("uf", "UF brasileira inválida.");
        }

        return valor;
    }
}
