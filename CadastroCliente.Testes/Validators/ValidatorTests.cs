using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Domain.Enums;
using FluentAssertions;

namespace CadastroCliente.Testes.Validators;

public class ValidatorTests
{
    [Fact]
    public void CriarCliente_invalido()
    {
        var validator = new CriarClienteRequestValidator();
        var result = validator.Validate(new CriarClienteRequest
        {
            Nome = "Al",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "123"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void CriarCliente_valido()
    {
        var validator = new CriarClienteRequestValidator();
        var result = validator.Validate(new CriarClienteRequest
        {
            Nome = "Maria Silva",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "52998224725"
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CriarCliente_rejeita_id_fornecido_pelo_cliente()
    {
        var validator = new CriarClienteRequestValidator();
        var result = validator.Validate(new CriarClienteRequest
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Silva",
            TipoPessoa = TipoPessoa.Fisica,
            Documento = "52998224725"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Endereco_e_contato_validators()
    {
        new EnderecoRequestValidator().Validate(new EnderecoRequest
        {
            Logradouro = "Rua das Flores",
            Numero = "10",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "01310100"
        }).IsValid.Should().BeTrue();

        new ContatoRequestValidator().Validate(new ContatoRequest
        {
            Tipo = TipoContato.Email,
            Valor = "nao-email"
        }).IsValid.Should().BeFalse();

        new AtualizarClienteRequestValidator().Validate(new AtualizarClienteRequest { Nome = "Maria Silva" })
            .IsValid.Should().BeTrue();
    }
}
