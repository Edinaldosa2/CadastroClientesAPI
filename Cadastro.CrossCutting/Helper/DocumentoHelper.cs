using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.ValueObjects;

namespace CadastroCliente.CrossCutting.Helper;

public static class DocumentoHelper
{
    public static string SomenteDigitos(string? valor) => Documento.SomenteDigitos(valor);

    public static bool EhCpfValido(string? cpf) => Documento.EhCpfValido(cpf);

    public static bool EhCnpjValido(string? cnpj) => Documento.EhCnpjValido(cnpj);

    public static bool EhValido(string? valor, TipoPessoa tipo)
        => tipo == TipoPessoa.Fisica ? EhCpfValido(valor) : EhCnpjValido(valor);

    public static string? Formatado(string? valor, TipoPessoa tipo)
    {
        try
        {
            return Documento.Criar(valor, tipo).Formatado();
        }
        catch
        {
            return valor;
        }
    }
}
