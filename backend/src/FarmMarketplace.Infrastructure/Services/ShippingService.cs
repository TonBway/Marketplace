using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Shipping;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class ShippingService : IShippingService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ShippingService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<ShippingMethodResponse>> GetMethodsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select shipping_method_id as ShippingMethodId, method_code as MethodCode, method_name as MethodName, description as Description
from catalog.shipping_methods where is_active = true order by sort_order, method_name";
        var rows = await connection.QueryAsync<ShippingMethodResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
