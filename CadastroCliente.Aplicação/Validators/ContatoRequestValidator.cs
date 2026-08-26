using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.CrossCutting.Helper;
using CadastroCliente.Domain.Enums;
using FluentValidation;

namespace CadastroCliente.Aplicacao.Validators;

public sealed class ContatoRequestValidator : AbstractValidator<ContatoRequest>
{
    public ContatoRequestValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Valor).NotEmpty();
        RuleFor(x => x.Valor)
            .EmailAddress()
            .When(x => x.Tipo == TipoContato.Email)
            .WithMessage("E-mail inválido.");
        RuleFor(x => x.Valor)
            .Must(TelefoneHelper.EhValido)
            .When(x => x.Tipo != TipoContato.Email)
            .WithMessage("Telefone deve ter DDD + número (10 a 13 dígitos).");
        RuleFor(x => x.Nome).MaximumLength(80);
    }
}
