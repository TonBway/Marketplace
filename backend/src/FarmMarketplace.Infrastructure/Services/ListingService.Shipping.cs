using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Contracts.Shipping;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

/// <summary>Shipping option management on listings (partial ListingService).</summary>
public sealed partial class ListingService
{
    public async Task<IReadOnlyList<ListingShippingOptionResponse>> GetShippingOptionsAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select sm.shipping_method_id as ShippingMethodId, sm.method_name as MethodName,
       lso.estimated_days as EstimatedDays, lso.cost as Cost, lso.notes as Notes
from marketplace.listing_shipping_options lso
inner join catalog.shipping_methods sm on sm.shipping_method_id = lso.shipping_method_id
where lso.listing_id = @ListingId
order by sm.sort_order, sm.method_name";
        var rows = await connection.QueryAsync<ListingShippingOptionResponse>(new CommandDefinition(sql, new { ListingId = listingId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task UpsertShippingOptionsAsync(Guid sellerUserId, Guid listingId, UpsertListingShippingOptionsRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verify ownership
        const string ownerSql = "select count(1) from marketplace.listings where listing_id = @ListingId and seller_user_id = @SellerUserId";
        var owns = await connection.ExecuteScalarAsync<int>(new CommandDefinition(ownerSql, new { ListingId = listingId, SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
        if (owns == 0) throw new InvalidOperationException("Listing not found.");

        // Delete then re-insert (simple upsert approach)
        const string deleteSql = "delete from marketplace.listing_shipping_options where listing_id = @ListingId";
        await connection.ExecuteAsync(new CommandDefinition(deleteSql, new { ListingId = listingId }, cancellationToken: cancellationToken));

        if (request.Options.Count > 0)
        {
            const string insertSql = @"
insert into marketplace.listing_shipping_options (listing_shipping_option_id, listing_id, shipping_method_id, estimated_days, cost, notes)
values (gen_random_uuid(), @ListingId, @ShippingMethodId, @EstimatedDays, @Cost, @Notes)";

            foreach (var option in request.Options)
            {
                await connection.ExecuteAsync(new CommandDefinition(insertSql, new
                {
                    ListingId = listingId,
                    option.ShippingMethodId,
                    option.EstimatedDays,
                    option.Cost,
                    option.Notes
                }, cancellationToken: cancellationToken));
            }
        }
    }
}
