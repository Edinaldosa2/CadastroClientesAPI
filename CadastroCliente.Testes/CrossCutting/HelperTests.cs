using CadastroCliente.CrossCutting.Helper;
using CadastroCliente.CrossCutting.Time;
using CadastroCliente.Domain.Enums;
using FluentAssertions;

namespace CadastroCliente.Testes.CrossCutting;

public class HelperTests
{
    [Fact]
    public void DocumentoHelper_formata_e_valida()
    {
        DocumentoHelper.EhValido("52998224725", TipoPessoa.Fisica).Should().BeTrue();
        DocumentoHelper.EhValido("11222333000181", TipoPessoa.Juridica).Should().BeTrue();
        DocumentoHelper.Formatado("52998224725", TipoPessoa.Fisica).Should().Be("529.982.247-25");
        DocumentoHelper.SomenteDigitos("529.982.247-25").Should().Be("52998224725");
    }

    [Fact]
    public void CepHelper_valida_e_formata()
    {
        CepHelper.EhValido("01310-100").Should().BeTrue();
        CepHelper.EhValido("123").Should().BeFalse();
        CepHelper.Formatado("01310100").Should().Be("01310-100");
    }

    [Fact]
    public void TelefoneHelper_valida_e_formata()
    {
        TelefoneHelper.EhValido("11987654321").Should().BeTrue();
        TelefoneHelper.EhValido("123").Should().BeFalse();
        TelefoneHelper.Formatado("11987654321").Should().Be("(11) 98765-4321");
        TelefoneHelper.Formatado("1133334444").Should().Be("(11) 3333-4444");
    }

    [Fact]
    public void TextoHelper_e_relogio()
    {
        TextoHelper.NuloSeVazio("  ").Should().BeNull();
        TextoHelper.Normalizar("  a  ").Should().Be("a");
        new SystemClock().UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }
}
