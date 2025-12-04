using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Liferay2WordPress.Services;

public record MigrationState
{
    public HashSet<string> CompletedArticleIds { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> CreatedSlugToWpId { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public interface IMigrationStateStore
{
    Task<MigrationState> LoadAsync(string path, CancellationToken ct);
    Task SaveAsync(string path, MigrationState state, CancellationToken ct);
}

public class FileMigrationStateStore : IMigrationStateStore
{
    private readonly ILogger<FileMigrationStateStore> _logger;

    public FileMigrationStateStore(ILogger<FileMigrationStateStore> logger)
    {
        _logger = logger;
    }

    public async Task<MigrationState> LoadAsync(string path, CancellationToken ct)
    {
        try
        {
            if (!File.Exists(path)) return new MigrationState();
            await using var s = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<MigrationState>(s, cancellationToken: ct);
            return state ?? new MigrationState();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load migration state from {Path}", path);
            return new MigrationState();
        }
    }

    public async Task SaveAsync(string path, MigrationState state, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        await using var s = File.Create(path);
        await JsonSerializer.SerializeAsync(s, state, cancellationToken: ct, options: new JsonSerializerOptions { WriteIndented = true });
    }
}
