using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Liferay2WordPress.Data;

public interface ILiferayUserRepository
{
    Task<(string ScreenName, string EmailAddress, string FullName)> GetUserAsync(long userId, CancellationToken ct);
}

public class LiferayUserRepository : ILiferayUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<LiferayUserRepository> _logger;

    public LiferayUserRepository(string connectionString, ILogger<LiferayUserRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<(string ScreenName, string EmailAddress, string FullName)> GetUserAsync(long userId, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var sql = @"SELECT screenName, emailAddress, firstName, middleName, lastName FROM user_ WHERE userId = @userId";
        var row = await conn.QuerySingleOrDefaultAsync(sql, new { userId });
        if (row == null) return ("user" + userId, $"user{userId}@example.com", "Liferay User");
        string fn = ((string?)row.firstName) ?? string.Empty;
        string mn = ((string?)row.middleName) ?? string.Empty;
        string ln = ((string?)row.lastName) ?? string.Empty;
        var full = string.Join(" ", new[] { fn, mn, ln }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return ((string)row.screenName, (string)row.emailAddress, string.IsNullOrWhiteSpace(full) ? (string)row.screenName : full);
    }
}
