using Dapper;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed partial class ListingService
{
    public async Task DeleteImageAsync(Guid sellerUserId, Guid listingId, Guid imageId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
delete from marketplace.listing_images li
using marketplace.listings l
where li.listing_image_id = @ImageId
  and li.listing_id = @ListingId
  and l.listing_id = li.listing_id
  and l.seller_user_id = @SellerUserId";

        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            SellerUserId = sellerUserId,
            ListingId = listingId,
            ImageId = imageId
        }, cancellationToken: cancellationToken));

        if (affected == 0)
        {
            throw new InvalidOperationException("Image not found.");
        }
    }

    public async Task<IReadOnlyList<ListingSummaryResponse>> GetFavoritesAsync(Guid buyerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select l.listing_id as ListingId, l.seller_user_id as SellerUserId, su.full_name as SellerName,
       l.title as Title, l.description as Description, l.price as Price,
       l.quantity as Quantity, u.unit_name as UnitName, ls.status_code as StatusCode, l.created_at_utc as CreatedAtUtc,
       l.expires_at_utc as ExpiresAtUtc,
       img.image_url as PrimaryImageUrl
from marketplace.listing_favorites f
inner join marketplace.listings l on l.listing_id = f.listing_id
inner join auth.users su on su.user_id = l.seller_user_id
inner join catalog.units u on u.unit_id = l.unit_id
inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
left join lateral (
    select li.image_url
    from marketplace.listing_images li
    where li.listing_id = l.listing_id
    order by li.is_primary desc, li.sort_order asc, li.created_at_utc asc
    limit 1
) img on true
where f.buyer_user_id = @BuyerUserId
order by f.created_at_utc desc";

        var rows = await connection.QueryAsync<ListingSummaryResponse>(new CommandDefinition(sql, new
        {
            BuyerUserId = buyerUserId
        }, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task AddFavoriteAsync(Guid buyerUserId, Guid listingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into marketplace.listing_favorites (listing_favorite_id, listing_id, buyer_user_id, created_at_utc)
values (gen_random_uuid(), @ListingId, @BuyerUserId, now())
on conflict (listing_id, buyer_user_id) do nothing";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            ListingId = listingId,
            BuyerUserId = buyerUserId
        }, cancellationToken: cancellationToken));
    }

    public async Task RemoveFavoriteAsync(Guid buyerUserId, Guid listingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
delete from marketplace.listing_favorites
where listing_id = @ListingId and buyer_user_id = @BuyerUserId";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            ListingId = listingId,
            BuyerUserId = buyerUserId
        }, cancellationToken: cancellationToken));
    }
}
