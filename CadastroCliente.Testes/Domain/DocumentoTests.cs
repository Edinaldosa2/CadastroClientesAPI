using CadastroCliente.Domain.Enums;
using CadastroCliente.Domain.Exceptions;
using CadastroCliente.Domain.ValueObjects;
using FluentAssertions;

namespace CadastroCliente.Testes.Domain;

public class DocumentoTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void Cpf_valido_e_formatado(string cpf)
    {
        var doc = Documento.Criar(cpf, TipoPessoa.Fisica);
        doc.Numero.Should().HaveLength(11);
        doc.Formatado().Should().MatchRegex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
        Documento.EhCpfValido(cpf).Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("123")]
    [InlineData("52998224724")]
    [InlineData("")]
    public void Cpf_invalido_lanca(string cpf)
    {
        var act = () => Documento.Criar(cpf, TipoPessoa.Fisica);
        act.Should().Throw<BusinessRuleException>().Which.Code.Should().Be("validation");
        Documento.EhCpfValido(cpf).Should().BeFalse();
    }

    [Fact]
    public void Cnpj_valido_e_formatado()
    {
        var doc = Documento.Criar("11.222.333/0001-81", TipoPessoa.Juridica);
        doc.Numero.Should().Be("11222333000181");
        doc.Formatado().Should().Be("11.222.333/0001-81");
        Documento.EhCnpjValido("11222333000181").Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11222333000180")]
    [InlineData("123")]
    public void Cnpj_invalido_lanca(string cnpj)
    {
        var act = () => Documento.Criar(cnpj, TipoPessoa.Juridica);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Igualdade_considera_numero_e_tipo()
    {
        var a = Documento.Criar("52998224725", TipoPessoa.Fisica);
        var b = Documento.Criar("529.982.247-25", TipoPessoa.Fisica);
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
        a.ToString().Should().Be(a.Formatado());
    }
}
