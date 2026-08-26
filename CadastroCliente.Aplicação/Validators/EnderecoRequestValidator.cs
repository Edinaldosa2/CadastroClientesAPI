using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.CrossCutting.Helper;
using FluentValidation;

namespace CadastroCliente.Aplicacao.Validators;

public sealed class EnderecoRequestValidator : AbstractValidator<EnderecoRequest>
{
    public EnderecoRequestValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Logradouro).NotEmpty().Length(5, 150);
        RuleFor(x => x.Numero).NotEmpty().Length(1, 20);
        RuleFor(x => x.Bairro).NotEmpty().Length(2, 80);
        RuleFor(x => x.Cidade).NotEmpty().Length(2, 80);
        RuleFor(x => x.Uf).NotEmpty().Length(2);
        RuleFor(x => x.Cep)
            .NotEmpty()
            .Must(CepHelper.EhValido)
            .WithMessage("CEP inválido.");
        RuleFor(x => x.Complemento).MaximumLength(80);
        RuleFor(x => x.Pais).MaximumLength(60);
    }
}
