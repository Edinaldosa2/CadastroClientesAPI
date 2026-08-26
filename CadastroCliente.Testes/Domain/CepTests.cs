using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.ValueObjects;
using FluentAssertions;

namespace CadastroCliente.Testes.Domain;

public class CepTests
{
    [Fact]
    public void Cep_valido()
    {
        var cep = Cep.Criar("01310-100");
        cep.Numero.Should().Be("01310100");
        cep.Formatado().Should().Be("01310-100");
        cep.ToString().Should().Be("01310-100");
        cep.Should().Be(Cep.Criar("01310100"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("00000000")]
    [InlineData("")]
    public void Cep_invalido(string valor)
    {
        var act = () => Cep.Criar(valor);
        act.Should().Throw<BusinessRuleException>();
    }
}
