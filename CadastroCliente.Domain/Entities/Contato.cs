using System.Net.Mail;
using System.Text.RegularExpressions;
using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;

namespace CadastroCliente.Domain.Entities;

public sealed class Contato : EntityBase
{
    private static readonly Regex TelefoneRegex = new(@"^\d{10,13}$", RegexOptions.Compiled);

    public Guid ClienteId { get; private set; }
    public TipoContato Tipo { get; private set; }
    public string Valor { get; private set; } = string.Empty;
    public string? Nome { get; private set; }
    public bool Principal { get; private set; }

    private Contato()
    {
    }

    internal Contato(
        Guid clienteId,
        TipoContato tipo,
        string valor,
        string? nome,
        bool principal,
        DateTimeOffset agora)
    {
        ClienteId = clienteId;
        Tipo = tipo;
        Valor = NormalizarValor(tipo, valor);
        Nome = string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
        Principal = principal;
        CriadoEm = agora.UtcDateTime;
    }

    internal void Atualizar(TipoContato tipo, string valor, string? nome, DateTimeOffset agora)
    {
        Tipo = tipo;
        Valor = NormalizarValor(tipo, valor);
        Nome = string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
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

    private static string NormalizarValor(TipoContato tipo, string? valor)
    {
        if (tipo == TipoContato.Email)
        {
            var email = valor?.Trim() ?? string.Empty;
            try
            {
                var parsed = new MailAddress(email);
                if (parsed.Address != email)
                {
                    throw new BusinessRuleException("valor", "E-mail inválido.");
                }
            }
            catch (FormatException)
            {
                throw new BusinessRuleException("valor", "E-mail inválido.");
            }

            if (email.Length > 160)
            {
                throw new BusinessRuleException("valor", "E-mail deve ter no máximo 160 caracteres.");
            }

            return email.ToLowerInvariant();
        }

        var digits = string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());

        if (!TelefoneRegex.IsMatch(digits))
        {
            throw new BusinessRuleException("valor", "Telefone deve ter DDD + número (10 a 13 dígitos).");
        }

        return digits;
    }
}
