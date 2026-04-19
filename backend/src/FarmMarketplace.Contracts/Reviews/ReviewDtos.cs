namespace FarmMarketplace.Contracts.Reviews;

public sealed record CreateReviewRequest(Guid ListingId, int Rating, string? Comment);

public sealed record ReviewResponse(
    Guid ReviewId,
    Guid ListingId,
    Guid ReviewerUserId,
    string ReviewerName,
    int Rating,
    string? Comment,
    DateTime CreatedAtUtc);

public sealed record SellerRatingSummaryResponse(
    double AverageRating,
    int TotalReviews,
    IReadOnlyList<ReviewResponse> RecentReviews);
