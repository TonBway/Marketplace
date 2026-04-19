using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Reviews;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReviewService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<ReviewResponse> CreateAsync(Guid buyerUserId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5.");

        using var connection = _connectionFactory.CreateConnection();

        const string listingSql = "select seller_user_id from marketplace.listings where listing_id = @ListingId";
        var sellerUserId = await connection.ExecuteScalarAsync<Guid?>(new CommandDefinition(listingSql,
            new { request.ListingId }, cancellationToken: cancellationToken));
        if (sellerUserId is null)
            throw new InvalidOperationException("Listing not found.");

        const string insertSql = @"
insert into marketplace.reviews (review_id, listing_id, reviewer_user_id, seller_user_id, rating, comment, created_at_utc)
values (gen_random_uuid(), @ListingId, @ReviewerUserId, @SellerUserId, @Rating, @Comment, now())
on conflict (listing_id, reviewer_user_id) do update set rating = excluded.rating, comment = excluded.comment
returning review_id";

        var reviewId = await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(insertSql, new
        {
            request.ListingId,
            ReviewerUserId = buyerUserId,
            SellerUserId = sellerUserId,
            request.Rating,
            request.Comment
        }, cancellationToken: cancellationToken));

        return (await GetByIdAsync(reviewId, cancellationToken))!;
    }

    private async Task<ReviewResponse?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select r.review_id as ReviewId, r.listing_id as ListingId, r.reviewer_user_id as ReviewerUserId,
       u.full_name as ReviewerName, r.rating as Rating, r.comment as Comment, r.created_at_utc as CreatedAtUtc
from marketplace.reviews r
inner join auth.users u on u.user_id = r.reviewer_user_id
where r.review_id = @ReviewId";
        return await connection.QuerySingleOrDefaultAsync<ReviewResponse>(new CommandDefinition(sql,
            new { ReviewId = reviewId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetForListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select r.review_id as ReviewId, r.listing_id as ListingId, r.reviewer_user_id as ReviewerUserId,
       u.full_name as ReviewerName, r.rating as Rating, r.comment as Comment, r.created_at_utc as CreatedAtUtc
from marketplace.reviews r
inner join auth.users u on u.user_id = r.reviewer_user_id
where r.listing_id = @ListingId and r.is_visible = true
order by r.created_at_utc desc";
        var rows = await connection.QueryAsync<ReviewResponse>(new CommandDefinition(sql,
            new { ListingId = listingId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<SellerRatingSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string avgSql = @"
select coalesce(avg(rating)::float, 0), count(1)
from marketplace.reviews
where seller_user_id = @SellerUserId and is_visible = true";
        var (avg, total) = await connection.QuerySingleAsync<(double, int)>(new CommandDefinition(avgSql,
            new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken));

        const string recentSql = @"
select r.review_id as ReviewId, r.listing_id as ListingId, r.reviewer_user_id as ReviewerUserId,
       u.full_name as ReviewerName, r.rating as Rating, r.comment as Comment, r.created_at_utc as CreatedAtUtc
from marketplace.reviews r
inner join auth.users u on u.user_id = r.reviewer_user_id
where r.seller_user_id = @SellerUserId and r.is_visible = true
order by r.created_at_utc desc
limit 5";
        var recent = (await connection.QueryAsync<ReviewResponse>(new CommandDefinition(recentSql,
            new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken))).ToList();

        return new SellerRatingSummaryResponse(Math.Round(avg, 1), total, recent);
    }
}
