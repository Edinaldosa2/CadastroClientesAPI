using CadastroCliente.Aplicacao.DTOs;
using FluentValidation;

namespace CadastroCliente.Aplicacao.Validators;

public sealed class AtualizarClienteRequestValidator : AbstractValidator<AtualizarClienteRequest>
{
    public AtualizarClienteRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .Length(3, 150);

        RuleFor(x => x.NomeFantasia).MaximumLength(150);
        RuleFor(x => x.InscricaoEstadual).MaximumLength(20);
        RuleFor(x => x.Observacoes).MaximumLength(1000);
    }
}
