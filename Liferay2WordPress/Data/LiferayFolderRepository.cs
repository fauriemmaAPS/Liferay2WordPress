using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Liferay2WordPress.Data;

public interface ILiferayFolderRepository
{
    Task<List<Liferay2WordPress.Models.LiferayFolder>> GetFoldersAsync(long groupId, CancellationToken ct);
}

public class LiferayFolderRepository : ILiferayFolderRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayFolderRepository> _logger;

    public LiferayFolderRepository(string connectionString, ILogger<LiferayFolderRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<List<Liferay2WordPress.Models.LiferayFolder>> GetFoldersAsync(long groupId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        var sql = @"SELECT folderId, name, parentFolderId FROM journalfolder WHERE groupId = @groupId";
        var rows = await conn.QueryAsync(sql, new { groupId });
        var list = new List<Liferay2WordPress.Models.LiferayFolder>();
        foreach (var r in rows)
        {
            list.Add(new Liferay2WordPress.Models.LiferayFolder((long)r.folderId, (string)r.name, (long)r.parentFolderId));
        }
        return list;
    }
}
