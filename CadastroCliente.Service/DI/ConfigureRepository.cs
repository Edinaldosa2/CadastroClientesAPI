using CadastroCliente.CrossCutting.Time;
using CadastroCliente.Data.Implementation;
using CadastroCliente.Domain.Interfaces;
using CadastroCliente.Service.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCliente.Service.DI;

public static class ConfigureRepository
{
    public static IServiceCollection AddCadastroRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddScoped<IUnitOfWork, DispatchingUnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
