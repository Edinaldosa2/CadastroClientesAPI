namespace CadastroCliente.Aplicacao.Abstractions;

public interface ICurrentUser
{
    bool Autenticado { get; }
    string? Usuario { get; }
    bool IsAdmin { get; }
    bool PodeEscrever { get; }
    bool PodeLer { get; }
    IReadOnlyCollection<string> Roles { get; }
}
