using CadastroCliente.Aplicacao.Interfaces;
using CadastroCliente.CrossCutting.Time;
using CadastroCliente.Data.Context;
using CadastroCliente.Data.Implementation;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Service.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCliente.Service.DI;

public static class ConfigureRepository
{
    public static IServiceCollection AddCadastroRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CadastroClienteContext>());
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
