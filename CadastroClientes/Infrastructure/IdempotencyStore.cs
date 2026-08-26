using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace CadastroClientes.Infrastructure;

public sealed class IdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _entries = new();

    public IdempotencyEntry? Get(string key) => _entries.TryGetValue(key, out var entry) ? entry : null;

    public void Set(string key, IdempotencyEntry entry) => _entries[key] = entry;

    public static string HashBody(string body)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
}

public sealed record IdempotencyEntry(int StatusCode, string ContentType, string Body, string RequestHash, DateTimeOffset CreatedAt);
