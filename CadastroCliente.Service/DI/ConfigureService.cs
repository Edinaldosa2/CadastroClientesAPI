using CadastroCliente.Aplicacao.DTOs;
using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.Aplicacao.Validators;
using CadastroCliente.Service.Service;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCliente.Service.DI;

public static class ConfigureService
{
    public static IServiceCollection AddCadastroServices(this IServiceCollection services)
    {
        services.AddScoped<IClienteAppService, ClienteService>();
        services.AddScoped<IEnderecoAppService, EnderecoService>();
        services.AddScoped<IContatoAppService, ContatoService>();
        services.AddScoped<IRelatorioAppService, RelatorioService>();
        services.AddScoped<IValidator<CriarClienteRequest>, CriarClienteRequestValidator>();
        services.AddScoped<IValidator<AtualizarClienteRequest>, AtualizarClienteRequestValidator>();
        services.AddScoped<IValidator<EnderecoRequest>, EnderecoRequestValidator>();
        services.AddScoped<IValidator<ContatoRequest>, ContatoRequestValidator>();
        return services;
    }
}
