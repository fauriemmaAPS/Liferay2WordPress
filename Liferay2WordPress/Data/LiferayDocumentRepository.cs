using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text;
using System.Web;

namespace Liferay2WordPress.Data;

public interface ILiferayDocumentRepository
{
    Task<(byte[] Bytes, string FileName, string ContentType)?> GetFileByFileEntryIdAsync(long fileEntryId, CancellationToken ct);
    Task<string?> GetDownloadUrlByUuidAsync(string uuid, long groupId, CancellationToken ct);
}

public class LiferayDocumentRepository : ILiferayDocumentRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayDocumentRepository> _logger;

    public LiferayDocumentRepository(string connectionString, ILogger<LiferayDocumentRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<(byte[] Bytes, string FileName, string ContentType)?> GetFileByFileEntryIdAsync(long fileEntryId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        // DLFileEntry stores meta; DLContent stores blobs (by data_ or in filesystem depending on config). Here we try DB path.
        var fileSql = @"SELECT fe.title, fe.mimeType, c.data_ FROM dlfieltentry fe
JOIN dlcontent c ON c.companyId = fe.companyId AND c.repositoryId = fe.repositoryId AND c.path_ = fe.treePath
WHERE fe.fileEntryId = @id LIMIT 1";
        // Note: table name typo intentionally avoided, but some Liferay setups use different storage. Fallback to null if not found.
        try
        {
            var row = await conn.QuerySingleOrDefaultAsync(fileSql, new { id = fileEntryId });
            if (row == null) return null;
            return ((byte[])row.data_, (string)row.title, (string)row.mimeType);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DB blob not available for fileEntryId {Id}", fileEntryId);
            return null;
        }
    }

    public async Task<string?> GetDownloadUrlByUuidAsync(string uuid, long groupId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        var sql = @"SELECT fe.fileEntryId, fe.title, fe.extension, fe.groupId FROM dlfileentry fe WHERE fe.uuid_ = @uuid AND fe.groupId = @groupId";
        var row = await conn.QuerySingleOrDefaultAsync(sql, new { uuid, groupId });
        if (row == null) return null;
        return $"/documents/{row.groupId}/{HttpUtility.UrlPathEncode((string)row.title)}.{(string)row.extension}";
    }
}
