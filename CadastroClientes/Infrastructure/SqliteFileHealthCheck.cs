using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CadastroClientes.Infrastructure;

public sealed class SqliteFileHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqliteFileHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var cs = _configuration.GetConnectionString("Default") ?? string.Empty;
        if (cs.Contains("Mode=Memory", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(HealthCheckResult.Healthy("SQLite em memória."));
        }

        var prefix = "Data Source=";
        var start = cs.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Connection string SQLite inválida."));
        }

        var path = cs[(start + prefix.Length)..].Split(';', 2)[0].Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Arquivo SQLite não configurado."));
        }

        var full = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Diretório do SQLite inexistente."));
        }

        var exists = File.Exists(full);
        var size = exists ? new FileInfo(full).Length : 0;
        var writable = dir is not null && new DirectoryInfo(dir).Exists;
        return Task.FromResult(writable
            ? HealthCheckResult.Healthy($"SQLite {(exists ? $"ok, {size} bytes" : "ainda não criado")}.")
            : HealthCheckResult.Unhealthy("SQLite sem permissão de escrita."));
    }
}
