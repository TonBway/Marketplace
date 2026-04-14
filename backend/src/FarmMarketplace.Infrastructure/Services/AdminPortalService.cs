using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Admin;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class AdminPortalService : IAdminPortalService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AdminPortalService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminDashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    (select count(1) from auth.users u inner join auth.roles r on r.role_id = u.role_id where r.role_code = 'SELLER') as TotalSellers,
    (select count(1) from auth.users u inner join auth.roles r on r.role_id = u.role_id where r.role_code = 'BUYER') as TotalBuyers,
    (select count(1) from marketplace.listings l inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id where ls.status_code = 'PUBLISHED') as TotalActiveListings,
    (select count(1) from marketplace.listings l inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id where ls.status_code = 'DRAFT') as TotalPendingListings,
    (select count(1) from marketplace.listings l inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id where ls.status_code = 'SOLD_OUT') as TotalSoldOutListings,
    (select count(1) from billing.seller_subscriptions ss inner join catalog.subscription_statuses cs on cs.subscription_status_id = ss.subscription_status_id where cs.status_code = 'ACTIVE') as TotalActiveSubscriptions,
    (select count(1) from billing.seller_subscriptions where end_date_utc >= now() and end_date_utc <= now() + interval '7 day') as SubscriptionsExpiringSoon,
    (select count(1) from messaging.enquiries) as TotalEnquiries";

        return await connection.QuerySingleAsync<AdminDashboardSummaryResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<RecentActivityItemResponse>> GetRecentActivityAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select entity_type as ActivityType, action as Title, occurred_at_utc as CreatedAtUtc, entity_id as RelatedEntityId
from audit.audit_logs
order by occurred_at_utc desc
limit 20";
        var rows = await connection.QueryAsync<RecentActivityItemResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AdminSellerRowResponse>> GetSellersAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    u.user_id as UserId,
    u.full_name as FullName,
    u.email as Email,
    u.phone as Phone,
    u.is_active as IsActive,
    coalesce(sp.is_verified, false) as IsVerified,
    (
      select cs.status_code
      from billing.seller_subscriptions ss
      inner join catalog.subscription_statuses cs on cs.subscription_status_id = ss.subscription_status_id
      where ss.seller_user_id = u.user_id
      order by ss.end_date_utc desc
      limit 1
    ) as SubscriptionStatus,
    (
      select count(1)
      from marketplace.listings l
      inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
      where l.seller_user_id = u.user_id and ls.status_code = 'PUBLISHED'
    ) as ActiveListingCount,
    u.created_at_utc as CreatedAtUtc
from auth.users u
inner join auth.roles r on r.role_id = u.role_id and r.role_code = 'SELLER'
left join seller.seller_profiles sp on sp.user_id = u.user_id
where (@Search is null or u.full_name ilike '%' || @Search || '%' or u.email ilike '%' || @Search || '%')
  and (@StatusCode is null or (@StatusCode = 'ACTIVE' and u.is_active = true) or (@StatusCode = 'INACTIVE' and u.is_active = false))
order by u.created_at_utc desc";

        var rows = await connection.QueryAsync<AdminSellerRowResponse>(new CommandDefinition(sql, new { Search = search, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<AdminSellerDetailResponse?> GetSellerAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    u.user_id as UserId,
    u.full_name as FullName,
    u.email as Email,
    u.phone as Phone,
    u.is_active as IsActive,
    coalesce(sp.is_verified, false) as IsVerified,
    sp.business_name as BusinessName,
    sp.description as Description,
    sp.region_id as RegionId,
    sp.district_id as DistrictId,
    sp.contact_mode as ContactMode,
    sp.profile_image_url as ProfileImageUrl,
    (select count(1) from marketplace.listings l where l.seller_user_id = u.user_id) as TotalListings,
    (select count(1) from messaging.enquiries e where e.seller_user_id = u.user_id) as TotalEnquiries,
    u.created_at_utc as CreatedAtUtc
from auth.users u
inner join auth.roles r on r.role_id = u.role_id and r.role_code = 'SELLER'
left join seller.seller_profiles sp on sp.user_id = u.user_id
where u.user_id = @SellerUserId";

        return await connection.QuerySingleOrDefaultAsync<AdminSellerDetailResponse>(new CommandDefinition(sql, new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminBuyerRowResponse>> GetBuyersAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    u.user_id as UserId,
    u.full_name as FullName,
    u.email as Email,
    u.phone as Phone,
    u.is_active as IsActive,
    (
      select count(1)
      from marketplace.listing_favorites f
      where f.buyer_user_id = u.user_id
    ) as FavoritesCount,
    (
      select count(1)
      from messaging.enquiries e
      where e.buyer_user_id = u.user_id
    ) as EnquiryCount,
    u.created_at_utc as CreatedAtUtc
from auth.users u
inner join auth.roles r on r.role_id = u.role_id and r.role_code = 'BUYER'
where (@Search is null or u.full_name ilike '%' || @Search || '%' or u.email ilike '%' || @Search || '%')
  and (@StatusCode is null or (@StatusCode = 'ACTIVE' and u.is_active = true) or (@StatusCode = 'INACTIVE' and u.is_active = false))
order by u.created_at_utc desc";
        var rows = await connection.QueryAsync<AdminBuyerRowResponse>(new CommandDefinition(sql, new { Search = search, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<AdminBuyerDetailResponse?> GetBuyerAsync(Guid buyerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    u.user_id as UserId,
    u.full_name as FullName,
    u.email as Email,
    u.phone as Phone,
    u.is_active as IsActive,
    bp.display_name as DisplayName,
    bp.region_id as RegionId,
    bp.district_id as DistrictId,
    coalesce(bp.receive_sms, false) as ReceiveSms,
    coalesce(bp.receive_push, false) as ReceivePush,
    (select count(1) from marketplace.listing_favorites f where f.buyer_user_id = u.user_id) as FavoritesCount,
    (select count(1) from messaging.enquiries e where e.buyer_user_id = u.user_id) as EnquiryCount,
    u.created_at_utc as CreatedAtUtc
from auth.users u
inner join auth.roles r on r.role_id = u.role_id and r.role_code = 'BUYER'
left join messaging.buyer_profiles bp on bp.user_id = u.user_id
where u.user_id = @BuyerUserId";

        return await connection.QuerySingleOrDefaultAsync<AdminBuyerDetailResponse>(new CommandDefinition(sql, new { BuyerUserId = buyerUserId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminListingRowResponse>> GetListingsAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select l.listing_id as ListingId, l.title as Title, l.seller_user_id as SellerUserId, u.full_name as SellerName,
       l.price as Price, l.quantity as Quantity, ls.status_code as StatusCode, l.created_at_utc as CreatedAtUtc
from marketplace.listings l
inner join auth.users u on u.user_id = l.seller_user_id
inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
where (@Search is null or l.title ilike '%' || @Search || '%' or u.full_name ilike '%' || @Search || '%')
  and (@StatusCode is null or ls.status_code = @StatusCode)
order by l.created_at_utc desc";
        var rows = await connection.QueryAsync<AdminListingRowResponse>(new CommandDefinition(sql, new { Search = search, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<AdminListingDetailResponse?> GetListingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select l.listing_id as ListingId, l.title as Title, l.description as Description, l.seller_user_id as SellerUserId,
       u.full_name as SellerName, l.category_id as CategoryId, l.product_type_id as ProductTypeId,
       l.price as Price, l.quantity as Quantity, l.unit_id as UnitId, l.region_id as RegionId,
       l.district_id as DistrictId, ls.status_code as StatusCode, l.is_livestock as IsLivestock,
       l.created_at_utc as CreatedAtUtc, l.expires_at_utc as ExpiresAtUtc,
       (select count(1) from messaging.enquiries e where e.listing_id = l.listing_id) as EnquiryCount,
       (select count(1) from marketplace.listing_views v where v.listing_id = l.listing_id) as ViewCount
from marketplace.listings l
inner join auth.users u on u.user_id = l.seller_user_id
inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
where l.listing_id = @ListingId";

        var core = await connection.QuerySingleOrDefaultAsync<AdminListingDetailCore>(new CommandDefinition(sql, new { ListingId = listingId }, cancellationToken: cancellationToken));
        if (core is null)
        {
            return null;
        }

        const string imageSql = @"
select image_url as ImageUrl, is_primary as IsPrimary, sort_order as SortOrder
from marketplace.listing_images
where listing_id = @ListingId
order by sort_order";
        var images = (await connection.QueryAsync<AdminListingImageResponse>(new CommandDefinition(imageSql, new { ListingId = listingId }, cancellationToken: cancellationToken))).ToList();

        const string historySql = @"
select status_code as StatusCode, note as Note, changed_by_user_id as ChangedByUserId, changed_at_utc as ChangedAtUtc
from marketplace.listing_status_history
where listing_id = @ListingId
order by changed_at_utc desc";
        var history = (await connection.QueryAsync<AdminListingStatusHistoryResponse>(new CommandDefinition(historySql, new { ListingId = listingId }, cancellationToken: cancellationToken))).ToList();

        return new AdminListingDetailResponse(
            core.ListingId,
            core.Title,
            core.Description,
            core.SellerUserId,
            core.SellerName,
            core.CategoryId,
            core.ProductTypeId,
            core.Price,
            core.Quantity,
            core.UnitId,
            core.RegionId,
            core.DistrictId,
            core.StatusCode,
            core.IsLivestock,
            core.CreatedAtUtc,
            core.ExpiresAtUtc,
            core.EnquiryCount,
            core.ViewCount,
            images,
            history);
    }

    public async Task<IReadOnlyList<AdminSubscriptionRowResponse>> GetSubscriptionsAsync(string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select ss.seller_subscription_id as SellerSubscriptionId, ss.seller_user_id as SellerUserId, u.full_name as SellerName,
       sp.plan_name as PlanName, cs.status_code as StatusCode, ss.start_date_utc as StartDateUtc, ss.end_date_utc as EndDateUtc
from billing.seller_subscriptions ss
inner join auth.users u on u.user_id = ss.seller_user_id
inner join billing.subscription_plans sp on sp.plan_id = ss.plan_id
inner join catalog.subscription_statuses cs on cs.subscription_status_id = ss.subscription_status_id
where (@StatusCode is null or cs.status_code = @StatusCode)
order by ss.end_date_utc desc";
        var rows = await connection.QueryAsync<AdminSubscriptionRowResponse>(new CommandDefinition(sql, new { StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<AdminSubscriptionDetailResponse?> GetSubscriptionAsync(Guid sellerSubscriptionId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select ss.seller_subscription_id as SellerSubscriptionId, ss.seller_user_id as SellerUserId, u.full_name as SellerName,
       ss.plan_id as PlanId, sp.plan_name as PlanName, cs.status_code as StatusCode,
       ss.start_date_utc as StartDateUtc, ss.end_date_utc as EndDateUtc,
       coalesce((select sum(p.amount) from billing.subscription_payments p where p.seller_subscription_id = ss.seller_subscription_id), 0) as TotalPaidAmount
from billing.seller_subscriptions ss
inner join auth.users u on u.user_id = ss.seller_user_id
inner join billing.subscription_plans sp on sp.plan_id = ss.plan_id
inner join catalog.subscription_statuses cs on cs.subscription_status_id = ss.subscription_status_id
where ss.seller_subscription_id = @SellerSubscriptionId";

        return await connection.QuerySingleOrDefaultAsync<AdminSubscriptionDetailResponse>(new CommandDefinition(sql, new { SellerSubscriptionId = sellerSubscriptionId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminPaymentRowResponse>> GetPaymentsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select sp.subscription_payment_id as SubscriptionPaymentId, sp.seller_subscription_id as SellerSubscriptionId,
       sp.amount as Amount, pm.method_name as PaymentMethod, ps.status_name as PaymentStatus,
       sp.paid_at_utc as PaidAtUtc, sp.created_at_utc as CreatedAtUtc
from billing.subscription_payments sp
inner join catalog.payment_methods pm on pm.payment_method_id = sp.payment_method_id
inner join catalog.payment_statuses ps on ps.payment_status_id = sp.payment_status_id
order by sp.created_at_utc desc";
        var rows = await connection.QueryAsync<AdminPaymentRowResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AdminInvoiceRowResponse>> GetInvoicesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select invoice_id as InvoiceId, seller_subscription_id as SellerSubscriptionId,
       invoice_number as InvoiceNumber, amount as Amount, issued_at_utc as IssuedAtUtc, due_at_utc as DueAtUtc
from billing.invoices
order by issued_at_utc desc";
        var rows = await connection.QueryAsync<AdminInvoiceRowResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AdminPlanRowResponse>> GetPlansAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select plan_id as PlanId, plan_name as PlanName, price_amount as PriceAmount, duration_days as DurationDays,
       max_active_listings as MaxActiveListings, is_active as IsActive, sort_order as SortOrder
from billing.subscription_plans
order by sort_order, plan_id";
        var rows = await connection.QueryAsync<AdminPlanRowResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<int> CreatePlanAsync(UpsertPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into billing.subscription_plans (plan_name, price_amount, duration_days, max_active_listings, is_active, sort_order, created_at_utc)
values (@PlanName, @PriceAmount, @DurationDays, @MaxActiveListings, true, @SortOrder, now())
returning plan_id";
        var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
        await WriteAuditAsync(actorUserId, "PLAN_CREATED", "SUBSCRIPTION_PLAN", id.ToString(), "{}", cancellationToken);
        return id;
    }

    public async Task UpdatePlanAsync(int planId, UpsertPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
update billing.subscription_plans
set plan_name = @PlanName,
    price_amount = @PriceAmount,
    duration_days = @DurationDays,
    max_active_listings = @MaxActiveListings,
    sort_order = @SortOrder
where plan_id = @PlanId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            PlanId = planId,
            request.PlanName,
            request.PriceAmount,
            request.DurationDays,
            request.MaxActiveListings,
            request.SortOrder
        }, cancellationToken: cancellationToken));

        await WriteAuditAsync(actorUserId, "PLAN_UPDATED", "SUBSCRIPTION_PLAN", planId.ToString(), "{}", cancellationToken);
    }

    public async Task DeactivatePlanAsync(int planId, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "update billing.subscription_plans set is_active = false where plan_id = @PlanId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { PlanId = planId }, cancellationToken: cancellationToken));
        await WriteAuditAsync(actorUserId, "PLAN_DEACTIVATED", "SUBSCRIPTION_PLAN", planId.ToString(), "{}", cancellationToken);
    }

    public async Task<IReadOnlyList<AdminEnquiryRowResponse>> GetEnquiriesAsync(string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select e.enquiry_id as EnquiryId, e.listing_id as ListingId, e.buyer_user_id as BuyerUserId,
       e.seller_user_id as SellerUserId, es.status_code as StatusCode, e.created_at_utc as CreatedAtUtc
from messaging.enquiries e
inner join catalog.enquiry_statuses es on es.enquiry_status_id = e.enquiry_status_id
where (@StatusCode is null or es.status_code = @StatusCode)
order by e.created_at_utc desc";
        var rows = await connection.QueryAsync<AdminEnquiryRowResponse>(new CommandDefinition(sql, new { StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<AdminEnquiryDetailResponse?> GetEnquiryAsync(Guid enquiryId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select e.enquiry_id as EnquiryId, e.listing_id as ListingId, e.buyer_user_id as BuyerUserId,
       e.seller_user_id as SellerUserId, es.status_code as StatusCode,
       e.message as Message, e.created_at_utc as CreatedAtUtc, e.updated_at_utc as UpdatedAtUtc
from messaging.enquiries e
inner join catalog.enquiry_statuses es on es.enquiry_status_id = e.enquiry_status_id
where e.enquiry_id = @EnquiryId";
        return await connection.QuerySingleOrDefaultAsync<AdminEnquiryDetailResponse>(new CommandDefinition(sql, new { EnquiryId = enquiryId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminReferenceItemResponse>> GetReferenceItemsAsync(string referenceType, CancellationToken cancellationToken)
    {
        var mapping = GetReferenceMapping(referenceType);
        using var connection = _connectionFactory.CreateConnection();
        var codeExpr = mapping.CodeColumn is null ? mapping.NameColumn : mapping.CodeColumn;
        var activeExpr = mapping.IsActiveColumn is null ? "true" : mapping.IsActiveColumn;
        var sql = $"select {mapping.IdColumn} as Id, {codeExpr} as Code, {mapping.NameColumn} as Name, {activeExpr} as IsActive from {mapping.TableName} order by {mapping.NameColumn}";
        var rows = await connection.QueryAsync<AdminReferenceItemResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<int> CreateReferenceItemAsync(string referenceType, UpsertReferenceItemRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var mapping = GetReferenceMapping(referenceType);
        using var connection = _connectionFactory.CreateConnection();
        string sql;
        object payload;

        if (mapping.CodeColumn is null)
        {
            sql = mapping.IsActiveColumn is null
                ? $"insert into {mapping.TableName} ({mapping.NameColumn}) values (@Name) returning {mapping.IdColumn}"
                : $"insert into {mapping.TableName} ({mapping.NameColumn}, {mapping.IsActiveColumn}) values (@Name, true) returning {mapping.IdColumn}";
            payload = new { request.Name };
        }
        else
        {
            sql = mapping.IsActiveColumn is null
                ? $"insert into {mapping.TableName} ({mapping.CodeColumn}, {mapping.NameColumn}) values (@Code, @Name) returning {mapping.IdColumn}"
                : $"insert into {mapping.TableName} ({mapping.CodeColumn}, {mapping.NameColumn}, {mapping.IsActiveColumn}) values (@Code, @Name, true) returning {mapping.IdColumn}";
            payload = new { Code = request.Code ?? request.Name.ToUpperInvariant().Replace(' ', '_'), request.Name };
        }

        var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, payload, cancellationToken: cancellationToken));
        await WriteAuditAsync(actorUserId, "REFERENCE_CREATED", referenceType.ToUpperInvariant(), id.ToString(), "{}", cancellationToken);
        return id;
    }

    public async Task UpdateReferenceItemAsync(string referenceType, int id, UpsertReferenceItemRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var mapping = GetReferenceMapping(referenceType);
        using var connection = _connectionFactory.CreateConnection();
        string sql;
        object payload;

        if (mapping.CodeColumn is null)
        {
            sql = $"update {mapping.TableName} set {mapping.NameColumn} = @Name where {mapping.IdColumn} = @Id";
            payload = new { Id = id, request.Name };
        }
        else
        {
            sql = $"update {mapping.TableName} set {mapping.CodeColumn} = @Code, {mapping.NameColumn} = @Name where {mapping.IdColumn} = @Id";
            payload = new { Id = id, Code = request.Code ?? request.Name.ToUpperInvariant().Replace(' ', '_'), request.Name };
        }

        await connection.ExecuteAsync(new CommandDefinition(sql, payload, cancellationToken: cancellationToken));
        await WriteAuditAsync(actorUserId, "REFERENCE_UPDATED", referenceType.ToUpperInvariant(), id.ToString(), "{}", cancellationToken);
    }

    public async Task DeactivateReferenceItemAsync(string referenceType, int id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var mapping = GetReferenceMapping(referenceType);
        if (mapping.IsActiveColumn is null)
        {
            throw new InvalidOperationException("Deactivation is not supported for this reference type.");
        }

        using var connection = _connectionFactory.CreateConnection();
        var sql = $"update {mapping.TableName} set {mapping.IsActiveColumn} = false where {mapping.IdColumn} = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        await WriteAuditAsync(actorUserId, "REFERENCE_DEACTIVATED", referenceType.ToUpperInvariant(), id.ToString(), "{}", cancellationToken);
    }

    public async Task<IReadOnlyList<AdminSystemSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select setting_key as Key, setting_value as Value, updated_at_utc as UpdatedAtUtc
from admin.system_settings
order by setting_key";
        var rows = await connection.QueryAsync<AdminSystemSettingResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task UpdateSettingAsync(string key, UpdateSystemSettingRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into admin.system_settings (system_setting_id, setting_key, setting_value, updated_by_user_id, updated_at_utc)
values (gen_random_uuid(), @SettingKey, @SettingValue, @UpdatedByUserId, now())
on conflict (setting_key) do update set
setting_value = excluded.setting_value,
updated_by_user_id = excluded.updated_by_user_id,
updated_at_utc = now()";

        await connection.ExecuteAsync(new CommandDefinition(sql, new { SettingKey = key, SettingValue = request.Value, UpdatedByUserId = actorUserId }, cancellationToken: cancellationToken));

        await WriteAuditAsync(actorUserId, "SYSTEM_SETTING_UPDATED", "SYSTEM_SETTING", key, "{}", cancellationToken);
    }

    public async Task<IReadOnlyList<AdminAuditLogResponse>> GetAuditLogsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select audit_log_id as AuditLogId, actor_user_id as ActorUserId, action as Action,
       entity_type as EntityType, entity_id as EntityId, occurred_at_utc as OccurredAtUtc
from audit.audit_logs
order by occurred_at_utc desc
limit 200";
        var rows = await connection.QueryAsync<AdminAuditLogResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AdminModerationActionResponse>> GetModerationActionsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select moderation_action_id as ModerationActionId, target_type as TargetType, target_id as TargetId,
       action_code as ActionCode, reason as Reason, acted_by_user_id as ActedByUserId, acted_at_utc as ActedAtUtc
from admin.moderation_actions
order by acted_at_utc desc
limit 200";
        var rows = await connection.QueryAsync<AdminModerationActionResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AdminNoteResponse>> GetAdminNotesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select admin_note_id as AdminNoteId, target_user_id as TargetUserId, note as Note,
       created_by_user_id as CreatedByUserId, created_at_utc as CreatedAtUtc
from admin.admin_notes
order by created_at_utc desc
limit 200";
        var rows = await connection.QueryAsync<AdminNoteResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task UpdateSellerStatusAsync(Guid sellerUserId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var isActive = actionCode is "REACTIVATE" or "APPROVE";

        using var connection = _connectionFactory.CreateConnection();
        const string sql = "update auth.users set is_active = @IsActive, updated_at_utc = now() where user_id = @UserId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { IsActive = isActive, UserId = sellerUserId }, cancellationToken: cancellationToken));

        await WriteModerationAsync("SELLER", sellerUserId.ToString(), actionCode, request.Reason, actorUserId, cancellationToken);
        await WriteAuditAsync(actorUserId, $"SELLER_{actionCode}", "SELLER", sellerUserId.ToString(), "{}", cancellationToken);
    }

    public async Task UpdateListingStatusAsync(Guid listingId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var targetStatus = actionCode switch
        {
            "APPROVE" => "PUBLISHED",
            "REACTIVATE" => "PUBLISHED",
            "REJECT" => "UNPUBLISHED",
            "SUSPEND" => "UNPUBLISHED",
            "FEATURE" => "PUBLISHED",
            "UNFEATURE" => "PUBLISHED",
            _ => "DRAFT"
        };

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
update marketplace.listings l
set listing_status_id = ls.listing_status_id, updated_at_utc = now()
from catalog.listing_statuses ls
where l.listing_id = @ListingId and ls.status_code = @TargetStatus";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { ListingId = listingId, TargetStatus = targetStatus }, cancellationToken: cancellationToken));

        await WriteModerationAsync("LISTING", listingId.ToString(), actionCode, request.Reason, actorUserId, cancellationToken);
        await WriteAuditAsync(actorUserId, $"LISTING_{actionCode}", "LISTING", listingId.ToString(), "{}", cancellationToken);
    }

    public async Task UpdateSubscriptionStatusAsync(Guid sellerSubscriptionId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var targetStatus = actionCode switch
        {
            "ACTIVATE" => "ACTIVE",
            "EXTEND" => "ACTIVE",
            "CANCEL" => "CANCELLED",
            "SUSPEND" => "CANCELLED",
            _ => "ACTIVE"
        };

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
update billing.seller_subscriptions ss
set subscription_status_id = cs.subscription_status_id
from catalog.subscription_statuses cs
where ss.seller_subscription_id = @SellerSubscriptionId and cs.status_code = @TargetStatus";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { SellerSubscriptionId = sellerSubscriptionId, TargetStatus = targetStatus }, cancellationToken: cancellationToken));

        await WriteModerationAsync("SUBSCRIPTION", sellerSubscriptionId.ToString(), actionCode, request.Reason, actorUserId, cancellationToken);
        await WriteAuditAsync(actorUserId, $"SUBSCRIPTION_{actionCode}", "SUBSCRIPTION", sellerSubscriptionId.ToString(), "{}", cancellationToken);
    }

    public async Task UpdateEnquiryStatusAsync(Guid enquiryId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var targetStatus = actionCode switch
        {
            "CLOSE" => "CLOSED",
            "REVIEW" => "IN_PROGRESS",
            _ => "IN_PROGRESS"
        };

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
update messaging.enquiries e
set enquiry_status_id = es.enquiry_status_id, updated_at_utc = now()
from catalog.enquiry_statuses es
where e.enquiry_id = @EnquiryId and es.status_code = @TargetStatus";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { EnquiryId = enquiryId, TargetStatus = targetStatus }, cancellationToken: cancellationToken));

        await WriteModerationAsync("ENQUIRY", enquiryId.ToString(), actionCode, request.Reason, actorUserId, cancellationToken);
        await WriteAuditAsync(actorUserId, $"ENQUIRY_{actionCode}", "ENQUIRY", enquiryId.ToString(), "{}", cancellationToken);
    }

    public Task AddSellerNoteAsync(Guid sellerUserId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken)
        => AddAdminNoteAsync(sellerUserId, request, actorUserId, cancellationToken);

    public Task AddBuyerNoteAsync(Guid buyerUserId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken)
        => AddAdminNoteAsync(buyerUserId, request, actorUserId, cancellationToken);

    public Task AddListingNoteAsync(Guid listingId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken)
        => AddAdminNoteAsync(null, new AddAdminNoteRequest($"Listing {listingId}: {request.Note}"), actorUserId, cancellationToken);

    private async Task AddAdminNoteAsync(Guid? targetUserId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into admin.admin_notes (admin_note_id, target_user_id, note, created_by_user_id, created_at_utc)
values (gen_random_uuid(), @TargetUserId, @Note, @CreatedByUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { TargetUserId = targetUserId, Note = request.Note, CreatedByUserId = actorUserId }, cancellationToken: cancellationToken));
    }

    private async Task WriteModerationAsync(string targetType, string targetId, string actionCode, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into admin.moderation_actions (moderation_action_id, target_type, target_id, action_code, reason, acted_by_user_id, acted_at_utc)
values (gen_random_uuid(), @TargetType, @TargetId, @ActionCode, @Reason, @ActedByUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            TargetType = targetType,
            TargetId = targetId,
            ActionCode = actionCode,
            Reason = reason,
            ActedByUserId = actorUserId
        }, cancellationToken: cancellationToken));
    }

    private async Task WriteAuditAsync(Guid actorUserId, string action, string entityType, string entityId, string payload, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into audit.audit_logs (audit_log_id, actor_user_id, action, entity_type, entity_id, payload, occurred_at_utc)
values (gen_random_uuid(), @ActorUserId, @Action, @EntityType, @EntityId, @Payload::jsonb, now())";
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Payload = payload
        }, cancellationToken: cancellationToken));
    }

    private static ReferenceMapping GetReferenceMapping(string referenceType)
    {
        return referenceType.ToLowerInvariant() switch
        {
            "regions" => new("catalog.regions", "region_id", null, "region_name", "is_active"),
            "districts" => new("catalog.districts", "district_id", null, "district_name", "is_active"),
            "categories" => new("catalog.categories", "category_id", "category_code", "category_name", "is_active"),
            "product-types" => new("catalog.product_types", "product_type_id", null, "product_type_name", "is_active"),
            "units" => new("catalog.units", "unit_id", "unit_code", "unit_name", null),
            "contact-modes" => new("catalog.contact_modes", "contact_mode_id", "mode_code", "mode_name", null),
            "payment-methods" => new("catalog.payment_methods", "payment_method_id", "method_code", "method_name", null),
            _ => throw new InvalidOperationException("Unsupported reference type.")
        };
    }

    private sealed record ReferenceMapping(string TableName, string IdColumn, string? CodeColumn, string NameColumn, string? IsActiveColumn);

    private sealed record AdminListingDetailCore(
        Guid ListingId,
        string Title,
        string Description,
        Guid SellerUserId,
        string SellerName,
        int CategoryId,
        int ProductTypeId,
        decimal Price,
        decimal Quantity,
        int UnitId,
        int RegionId,
        int DistrictId,
        string StatusCode,
        bool IsLivestock,
        DateTime CreatedAtUtc,
        DateTime? ExpiresAtUtc,
        int EnquiryCount,
        int ViewCount);
}
