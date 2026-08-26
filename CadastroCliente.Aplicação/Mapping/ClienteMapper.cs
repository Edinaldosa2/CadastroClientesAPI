using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.CrossCutting.Helper;
using CadastroCliente.Domain.Entities;
using CadastroCliente.Domain.Enums;

namespace CadastroCliente.Aplicacao.Mapping;

public static class ClienteMapper
{
    public static ClienteDto ToDto(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        NomeFantasia = cliente.NomeFantasia,
        TipoPessoa = cliente.TipoPessoa,
        Documento = cliente.Documento,
        DocumentoFormatado = DocumentoHelper.Formatado(cliente.Documento, cliente.TipoPessoa) ?? cliente.Documento,
        InscricaoEstadual = cliente.InscricaoEstadual,
        DataNascimento = cliente.DataNascimento,
        Status = cliente.Status,
        MotivoStatus = cliente.MotivoStatus,
        Observacoes = cliente.Observacoes,
        Excluido = cliente.Excluido,
        CriadoEm = cliente.CriadoEm,
        AtualizadoEm = cliente.AtualizadoEm,
        Enderecos = cliente.Enderecos.Select(ToDto).ToList(),
        Contatos = cliente.Contatos.Select(ToDto).ToList()
    };

    public static ClienteListItemDto ToListItem(Cliente cliente)
    {
        var principal = cliente.Enderecos.FirstOrDefault(e => e.Principal) ?? cliente.Enderecos.FirstOrDefault();
        return new ClienteListItemDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            NomeFantasia = cliente.NomeFantasia,
            TipoPessoa = cliente.TipoPessoa,
            Documento = cliente.Documento,
            DocumentoFormatado = DocumentoHelper.Formatado(cliente.Documento, cliente.TipoPessoa) ?? cliente.Documento,
            Status = cliente.Status,
            Cidade = principal?.Cidade,
            Uf = principal?.Uf,
            CriadoEm = cliente.CriadoEm
        };
    }

    public static EnderecoDto ToDto(Endereco endereco) => new()
    {
        Id = endereco.Id,
        Tipo = endereco.Tipo,
        Logradouro = endereco.Logradouro,
        Numero = endereco.Numero,
        Complemento = endereco.Complemento,
        Bairro = endereco.Bairro,
        Cidade = endereco.Cidade,
        Uf = endereco.Uf,
        Cep = endereco.Cep,
        CepFormatado = CepHelper.Formatado(endereco.Cep),
        Pais = endereco.Pais,
        Principal = endereco.Principal
    };

    public static ContatoDto ToDto(Contato contato) => new()
    {
        Id = contato.Id,
        Tipo = contato.Tipo,
        Valor = contato.Valor,
        ValorFormatado = contato.Tipo == TipoContato.Email ? contato.Valor : TelefoneHelper.Formatado(contato.Valor),
        Nome = contato.Nome,
        Principal = contato.Principal
    };
}
