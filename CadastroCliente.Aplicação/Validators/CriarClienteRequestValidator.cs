using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.CrossCutting.Helper;
using CadastroCliente.Domain.Enums;
using FluentValidation;

namespace CadastroCliente.Aplicacao.Validators;

public sealed class CriarClienteRequestValidator : AbstractValidator<CriarClienteRequest>
{
    public CriarClienteRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .Length(3, 150).WithMessage("Nome deve ter entre 3 e 150 caracteres.");

        RuleFor(x => x.TipoPessoa)
            .IsInEnum().WithMessage("Tipo de pessoa inválido.");

        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("Documento é obrigatório.")
            .Must((req, doc) => DocumentoHelper.EhValido(doc, req.TipoPessoa))
            .WithMessage(req => req.TipoPessoa == TipoPessoa.Fisica ? "CPF inválido." : "CNPJ inválido.");

        RuleFor(x => x.NomeFantasia)
            .MaximumLength(150);

        RuleFor(x => x.InscricaoEstadual)
            .MaximumLength(20);

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000);

        RuleForEach(x => x.Enderecos).SetValidator(new EnderecoRequestValidator());
        RuleForEach(x => x.Contatos).SetValidator(new ContatoRequestValidator());
    }
}
