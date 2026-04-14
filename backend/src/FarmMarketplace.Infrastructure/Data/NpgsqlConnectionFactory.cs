using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FarmMarketplace.Infrastructure.Data;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MarketplaceDb")
            ?? throw new InvalidOperationException("Connection string 'MarketplaceDb' is missing.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
