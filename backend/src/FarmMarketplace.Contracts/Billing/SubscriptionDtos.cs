namespace FarmMarketplace.Contracts.Billing;

public sealed record SubscriptionPlanResponse(int PlanId, string PlanName, decimal PriceAmount, int DurationDays);

public sealed record SubscribeRequest(int PlanId, string PaymentMethodCode);

public sealed record ActiveSubscriptionResponse(Guid SellerSubscriptionId, int PlanId, string PlanName, DateTime StartDateUtc, DateTime EndDateUtc, string StatusCode);
