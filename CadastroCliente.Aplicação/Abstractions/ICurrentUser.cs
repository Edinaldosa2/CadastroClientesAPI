namespace CadastroCliente.Aplicacao.Abstractions;

public interface ICurrentUser
{
    bool Autenticado { get; }
    string? Usuario { get; }
}
