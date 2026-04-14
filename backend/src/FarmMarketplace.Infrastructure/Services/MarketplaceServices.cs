using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Auth;
using FarmMarketplace.Contracts.Billing;
using FarmMarketplace.Contracts.Buyer;
using FarmMarketplace.Contracts.Dashboard;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Contracts.Messaging;
using FarmMarketplace.Contracts.Reference;
using FarmMarketplace.Contracts.Seller;
using FarmMarketplace.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;

    public AuthService(IDbConnectionFactory connectionFactory, IConfiguration configuration)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string existingSql = "select count(1) from auth.users where lower(email) = lower(@Email) or phone = @Phone";
        var existing = await connection.ExecuteScalarAsync<int>(new CommandDefinition(existingSql, new { request.Email, request.Phone }, cancellationToken: cancellationToken));
        if (existing > 0)
        {
            throw new InvalidOperationException("User already exists.");
        }

        var userId = Guid.NewGuid();
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        const string insertUserSql = @"
insert into auth.users (user_id, full_name, email, phone, password_hash, role_id, is_active, created_at_utc)
select @UserId, @FullName, @Email, @Phone, @PasswordHash, r.role_id, true, now()
from auth.roles r
where r.role_code = @RoleCode";

        var rows = await connection.ExecuteAsync(new CommandDefinition(insertUserSql, new
        {
            UserId = userId,
            request.FullName,
            request.Email,
            request.Phone,
            PasswordHash = hash,
            request.RoleCode
        }, cancellationToken: cancellationToken));

        if (rows == 0)
        {
            throw new InvalidOperationException("Invalid role code.");
        }

        return await IssueTokensAsync(userId, request.FullName, request.Email, request.RoleCode, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
select u.user_id as UserId, u.full_name as FullName, u.email as Email, u.password_hash as PasswordHash, r.role_code as RoleCode
from auth.users u
inner join auth.roles r on r.role_id = u.role_id
where lower(u.email) = lower(@Value) or u.phone = @Value";

        var user = await connection.QuerySingleOrDefaultAsync<LoginRow>(new CommandDefinition(sql, new { Value = request.EmailOrPhone }, cancellationToken: cancellationToken));
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await IssueTokensAsync(user.UserId, user.FullName, user.Email, user.RoleCode, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
select rt.user_id as UserId, u.full_name as FullName, u.email as Email, r.role_code as RoleCode
from auth.refresh_tokens rt
inner join auth.users u on u.user_id = rt.user_id
inner join auth.roles r on r.role_id = u.role_id
where rt.token = @Token and rt.is_revoked = false and rt.expires_at_utc > now()";

        var row = await connection.QuerySingleOrDefaultAsync<RefreshRow>(new CommandDefinition(sql, new { Token = request.RefreshToken }, cancellationToken: cancellationToken));
        if (row is null)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        const string revokeSql = "update auth.refresh_tokens set is_revoked = true, revoked_at_utc = now() where token = @Token";
        await connection.ExecuteAsync(new CommandDefinition(revokeSql, new { Token = request.RefreshToken }, cancellationToken: cancellationToken));

        return await IssueTokensAsync(row.UserId, row.FullName, row.Email, row.RoleCode, cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(Guid userId, string fullName, string email, string roleCode, CancellationToken cancellationToken)
    {
        var accessTokenMinutes = _configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 60;
        var refreshTokenDays = _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 30;
        var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var jwt = CreateJwt(userId, fullName, email, roleCode, expiresAt);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into auth.refresh_tokens (refresh_token_id, user_id, token, expires_at_utc, is_revoked, created_at_utc)
values (@RefreshTokenId, @UserId, @Token, @ExpiresAtUtc, false, now())";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays)
        }, cancellationToken: cancellationToken));

        return new AuthResponse(userId, fullName, email, roleCode, jwt, refreshToken, expiresAt);
    }

    private string CreateJwt(Guid userId, string fullName, string email, string roleCode, DateTime expiresAt)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "FarmMarketplace";
        var audience = _configuration["Jwt:Audience"] ?? "FarmMarketplaceClients";
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, roleCode)
        };

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record LoginRow(Guid UserId, string FullName, string Email, string PasswordHash, string RoleCode);
    private sealed record RefreshRow(Guid UserId, string FullName, string Email, string RoleCode);
}

public sealed class SellerProfileService : ISellerProfileService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SellerProfileService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<SellerProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select user_id as UserId, business_name as BusinessName, description as Description, region_id as RegionId,
       district_id as DistrictId, contact_mode as ContactMode, profile_image_url as ProfileImageUrl, is_verified as IsVerified
from seller.seller_profiles where user_id = @UserId";
        return await connection.QuerySingleOrDefaultAsync<SellerProfileResponse>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<SellerProfileResponse> UpsertAsync(Guid userId, UpsertSellerProfileRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into seller.seller_profiles (seller_profile_id, user_id, business_name, description, region_id, district_id, contact_mode, profile_image_url, is_verified, created_at_utc, updated_at_utc)
values (gen_random_uuid(), @UserId, @BusinessName, @Description, @RegionId, @DistrictId, @ContactMode, @ProfileImageUrl, false, now(), now())
on conflict (user_id) do update set
business_name = excluded.business_name,
description = excluded.description,
region_id = excluded.region_id,
district_id = excluded.district_id,
contact_mode = excluded.contact_mode,
profile_image_url = excluded.profile_image_url,
updated_at_utc = now()";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = userId,
            request.BusinessName,
            request.Description,
            request.RegionId,
            request.DistrictId,
            request.ContactMode,
            request.ProfileImageUrl
        }, cancellationToken: cancellationToken));

        return (await GetAsync(userId, cancellationToken))!;
    }
}

public sealed class BuyerProfileService : IBuyerProfileService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BuyerProfileService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<BuyerProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select user_id as UserId, display_name as DisplayName, region_id as RegionId, district_id as DistrictId,
       receive_sms as ReceiveSms, receive_push as ReceivePush
from messaging.buyer_profiles where user_id = @UserId";
        return await connection.QuerySingleOrDefaultAsync<BuyerProfileResponse>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<BuyerProfileResponse> UpsertAsync(Guid userId, UpsertBuyerProfileRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
insert into messaging.buyer_profiles (buyer_profile_id, user_id, display_name, region_id, district_id, receive_sms, receive_push, created_at_utc, updated_at_utc)
values (gen_random_uuid(), @UserId, @DisplayName, @RegionId, @DistrictId, @ReceiveSms, @ReceivePush, now(), now())
on conflict (user_id) do update set
display_name = excluded.display_name,
region_id = excluded.region_id,
district_id = excluded.district_id,
receive_sms = excluded.receive_sms,
receive_push = excluded.receive_push,
updated_at_utc = now()";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            UserId = userId,
            request.DisplayName,
            request.RegionId,
            request.DistrictId,
            request.ReceiveSms,
            request.ReceivePush
        }, cancellationToken: cancellationToken));

        return (await GetAsync(userId, cancellationToken))!;
    }
}

public sealed class ListingService : IListingService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ListingService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateAsync(Guid sellerUserId, CreateListingRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var listingId = Guid.NewGuid();

        const string sql = @"
insert into marketplace.listings (listing_id, seller_user_id, title, description, category_id, product_type_id, price, quantity, unit_id, region_id, district_id, listing_status_id, is_livestock, created_at_utc, updated_at_utc)
select @ListingId, @SellerUserId, @Title, @Description, @CategoryId, @ProductTypeId, @Price, @Quantity, @UnitId, @RegionId, @DistrictId, s.listing_status_id, @IsLivestock, now(), now()
from catalog.listing_statuses s
where s.status_code = 'DRAFT'";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            ListingId = listingId,
            SellerUserId = sellerUserId,
            request.Title,
            request.Description,
            request.CategoryId,
            request.ProductTypeId,
            request.Price,
            request.Quantity,
            request.UnitId,
            request.RegionId,
            request.DistrictId,
            request.IsLivestock
        }, cancellationToken: cancellationToken));

        return listingId;
    }

    public async Task<IReadOnlyList<ListingSummaryResponse>> GetMyListingsAsync(Guid sellerUserId, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select l.listing_id as ListingId, l.seller_user_id as SellerUserId, l.title as Title, l.price as Price,
       l.quantity as Quantity, u.unit_name as UnitName, ls.status_code as StatusCode, l.created_at_utc as CreatedAtUtc,
       l.expires_at_utc as ExpiresAtUtc
from marketplace.listings l
inner join catalog.units u on u.unit_id = l.unit_id
inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
where l.seller_user_id = @SellerUserId and (@StatusCode is null or ls.status_code = @StatusCode)
order by l.created_at_utc desc";

        var rows = await connection.QueryAsync<ListingSummaryResponse>(new CommandDefinition(sql, new { SellerUserId = sellerUserId, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task UpdateStatusAsync(Guid sellerUserId, Guid listingId, UpdateListingStatusRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
update marketplace.listings l
set listing_status_id = s.listing_status_id, updated_at_utc = now()
from catalog.listing_statuses s
where l.listing_id = @ListingId and l.seller_user_id = @SellerUserId and s.status_code = @StatusCode";

        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { ListingId = listingId, SellerUserId = sellerUserId, StatusCode = request.StatusCode }, cancellationToken: cancellationToken));
        if (affected == 0)
        {
            throw new InvalidOperationException("Listing not found or status code is invalid.");
        }

        const string historySql = @"
insert into marketplace.listing_status_history (listing_status_history_id, listing_id, status_code, note, changed_by_user_id, changed_at_utc)
values (gen_random_uuid(), @ListingId, @StatusCode, null, @SellerUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(historySql, new { ListingId = listingId, StatusCode = request.StatusCode, SellerUserId = sellerUserId }, cancellationToken: cancellationToken));

        const string auditSql = @"
    insert into audit.audit_logs (audit_log_id, actor_user_id, action, entity_type, entity_id, payload, occurred_at_utc)
    values (gen_random_uuid(), @ActorUserId, @Action, @EntityType, @EntityId, @Payload::jsonb, now())";
        await connection.ExecuteAsync(new CommandDefinition(auditSql, new
        {
            ActorUserId = sellerUserId,
            Action = "LISTING_STATUS_UPDATED",
            EntityType = "LISTING",
            EntityId = listingId.ToString(),
            Payload = "{\"statusCode\":\"" + request.StatusCode + "\"}"
        }, cancellationToken: cancellationToken));
    }

    public async Task AddImageAsync(Guid sellerUserId, UploadListingImageRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string ownershipSql = "select count(1) from marketplace.listings where listing_id = @ListingId and seller_user_id = @SellerUserId";
        var owns = await connection.ExecuteScalarAsync<int>(new CommandDefinition(ownershipSql, new { request.ListingId, SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
        if (owns == 0)
        {
            throw new InvalidOperationException("Listing not found.");
        }

        const string sql = @"
insert into marketplace.listing_images (listing_image_id, listing_id, image_url, is_primary, sort_order, created_at_utc)
values (gen_random_uuid(), @ListingId, @ImageUrl, @IsPrimary, @SortOrder, now())";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { request.ListingId, request.ImageUrl, request.IsPrimary, request.SortOrder }, cancellationToken: cancellationToken));

        const string auditSql = @"
    insert into audit.audit_logs (audit_log_id, actor_user_id, action, entity_type, entity_id, payload, occurred_at_utc)
    values (gen_random_uuid(), @ActorUserId, @Action, @EntityType, @EntityId, @Payload::jsonb, now())";
        await connection.ExecuteAsync(new CommandDefinition(auditSql, new
        {
            ActorUserId = sellerUserId,
            Action = "LISTING_IMAGE_ADDED",
            EntityType = "LISTING",
            EntityId = request.ListingId.ToString(),
            Payload = "{\"imageUrl\":\"" + request.ImageUrl + "\"}"
        }, cancellationToken: cancellationToken));
    }
}

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SubscriptionService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<SubscriptionPlanResponse>> GetPlansAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "select plan_id as PlanId, plan_name as PlanName, price_amount as PriceAmount, duration_days as DurationDays from billing.subscription_plans where is_active = true order by sort_order";
        var rows = await connection.QueryAsync<SubscriptionPlanResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<ActiveSubscriptionResponse?> GetActiveForSellerAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select ss.seller_subscription_id as SellerSubscriptionId, ss.plan_id as PlanId, sp.plan_name as PlanName,
       ss.start_date_utc as StartDateUtc, ss.end_date_utc as EndDateUtc, cs.status_code as StatusCode
from billing.seller_subscriptions ss
inner join billing.subscription_plans sp on sp.plan_id = ss.plan_id
inner join catalog.subscription_statuses cs on cs.subscription_status_id = ss.subscription_status_id
where ss.seller_user_id = @SellerUserId and ss.end_date_utc >= now()
order by ss.end_date_utc desc
limit 1";
        return await connection.QuerySingleOrDefaultAsync<ActiveSubscriptionResponse>(new CommandDefinition(sql, new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
    }

    public async Task<ActiveSubscriptionResponse> SubscribeAsync(Guid sellerUserId, SubscribeRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string planSql = "select duration_days from billing.subscription_plans where plan_id = @PlanId and is_active = true";
        var durationDays = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(planSql, new { request.PlanId }, cancellationToken: cancellationToken));
        if (durationDays is null)
        {
            throw new InvalidOperationException("Plan not found.");
        }

        var start = DateTime.UtcNow;
        var end = start.AddDays(durationDays.Value);

        const string insertSql = @"
insert into billing.seller_subscriptions (seller_subscription_id, seller_user_id, plan_id, subscription_status_id, start_date_utc, end_date_utc, created_at_utc)
select gen_random_uuid(), @SellerUserId, @PlanId, ss.subscription_status_id, @StartDateUtc, @EndDateUtc, now()
from catalog.subscription_statuses ss
where ss.status_code = 'ACTIVE'";

        await connection.ExecuteAsync(new CommandDefinition(insertSql, new
        {
            SellerUserId = sellerUserId,
            request.PlanId,
            StartDateUtc = start,
            EndDateUtc = end
        }, cancellationToken: cancellationToken));

        const string auditSql = @"
insert into audit.audit_logs (audit_log_id, actor_user_id, action, entity_type, entity_id, payload, occurred_at_utc)
values (gen_random_uuid(), @ActorUserId, @Action, @EntityType, @EntityId, @Payload::jsonb, now())";
        await connection.ExecuteAsync(new CommandDefinition(auditSql, new
        {
            ActorUserId = sellerUserId,
            Action = "SUBSCRIPTION_CREATED",
            EntityType = "SELLER_SUBSCRIPTION",
            EntityId = request.PlanId.ToString(),
            Payload = "{\"planId\":" + request.PlanId + "}"
        }, cancellationToken: cancellationToken));

        return (await GetActiveForSellerAsync(sellerUserId, cancellationToken))!;
    }
}

public sealed class EnquiryService : IEnquiryService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EnquiryService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<EnquiryResponse> CreateAsync(Guid buyerUserId, CreateEnquiryRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var enquiryId = Guid.NewGuid();

        const string sql = @"
insert into messaging.enquiries (enquiry_id, listing_id, buyer_user_id, seller_user_id, enquiry_status_id, message, preferred_contact_mode, created_at_utc)
select @EnquiryId, l.listing_id, @BuyerUserId, l.seller_user_id, es.enquiry_status_id, @Message, @PreferredContactMode, now()
from marketplace.listings l
cross join catalog.enquiry_statuses es
where l.listing_id = @ListingId and es.status_code = 'NEW'";

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            EnquiryId = enquiryId,
            request.ListingId,
            BuyerUserId = buyerUserId,
            request.Message,
            request.PreferredContactMode
        }, cancellationToken: cancellationToken));

        const string getSql = @"
select e.enquiry_id as EnquiryId, e.listing_id as ListingId, e.buyer_user_id as BuyerUserId, e.seller_user_id as SellerUserId,
       s.status_code as StatusCode, e.message as Message, e.created_at_utc as CreatedAtUtc
from messaging.enquiries e
inner join catalog.enquiry_statuses s on s.enquiry_status_id = e.enquiry_status_id
where e.enquiry_id = @EnquiryId";

        return (await connection.QuerySingleOrDefaultAsync<EnquiryResponse>(new CommandDefinition(getSql, new { EnquiryId = enquiryId }, cancellationToken: cancellationToken)))
            ?? throw new InvalidOperationException("Failed to create enquiry.");
    }

    public async Task<IReadOnlyList<EnquiryResponse>> GetForSellerAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select e.enquiry_id as EnquiryId, e.listing_id as ListingId, e.buyer_user_id as BuyerUserId, e.seller_user_id as SellerUserId,
       s.status_code as StatusCode, e.message as Message, e.created_at_utc as CreatedAtUtc
from messaging.enquiries e
inner join catalog.enquiry_statuses s on s.enquiry_status_id = e.enquiry_status_id
where e.seller_user_id = @SellerUserId
order by e.created_at_utc desc";
        var rows = await connection.QueryAsync<EnquiryResponse>(new CommandDefinition(sql, new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<EnquiryResponse>> GetForBuyerAsync(Guid buyerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select e.enquiry_id as EnquiryId, e.listing_id as ListingId, e.buyer_user_id as BuyerUserId, e.seller_user_id as SellerUserId,
       s.status_code as StatusCode, e.message as Message, e.created_at_utc as CreatedAtUtc
from messaging.enquiries e
inner join catalog.enquiry_statuses s on s.enquiry_status_id = e.enquiry_status_id
where e.buyer_user_id = @BuyerUserId
order by e.created_at_utc desc";
        var rows = await connection.QueryAsync<EnquiryResponse>(new CommandDefinition(sql, new { BuyerUserId = buyerUserId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task UpdateStatusAsync(Guid sellerUserId, Guid enquiryId, UpdateEnquiryStatusRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string updateSql = @"
update messaging.enquiries e
set enquiry_status_id = s.enquiry_status_id, updated_at_utc = now()
from catalog.enquiry_statuses s
where e.enquiry_id = @EnquiryId and e.seller_user_id = @SellerUserId and s.status_code = @StatusCode";

        var affected = await connection.ExecuteAsync(new CommandDefinition(updateSql, new { EnquiryId = enquiryId, SellerUserId = sellerUserId, StatusCode = request.StatusCode }, cancellationToken: cancellationToken));
        if (affected == 0)
        {
            throw new InvalidOperationException("Enquiry not found or status code is invalid.");
        }

        const string historySql = @"
insert into messaging.enquiry_status_history (enquiry_status_history_id, enquiry_id, status_code, note, changed_by_user_id, changed_at_utc)
values (gen_random_uuid(), @EnquiryId, @StatusCode, @Note, @SellerUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(historySql, new { EnquiryId = enquiryId, StatusCode = request.StatusCode, request.Note, SellerUserId = sellerUserId }, cancellationToken: cancellationToken));

        const string auditSql = @"
    insert into audit.audit_logs (audit_log_id, actor_user_id, action, entity_type, entity_id, payload, occurred_at_utc)
    values (gen_random_uuid(), @ActorUserId, @Action, @EntityType, @EntityId, @Payload::jsonb, now())";
        await connection.ExecuteAsync(new CommandDefinition(auditSql, new
        {
            ActorUserId = sellerUserId,
            Action = "ENQUIRY_STATUS_UPDATED",
            EntityType = "ENQUIRY",
            EntityId = enquiryId.ToString(),
            Payload = "{\"statusCode\":\"" + request.StatusCode + "\"}"
        }, cancellationToken: cancellationToken));
    }
}

public sealed class NotificationService : INotificationService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<NotificationResponse>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select notification_id as NotificationId, user_id as UserId, title as Title, body as Body, is_read as IsRead, created_at_utc as CreatedAtUtc
from messaging.notifications
where user_id = @UserId
order by created_at_utc desc";
        var rows = await connection.QueryAsync<NotificationResponse>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReferenceDataService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<RegionResponse>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "select region_id as RegionId, region_name as RegionName from catalog.regions order by region_name";
        var rows = await connection.QueryAsync<RegionResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<DistrictResponse>> GetDistrictsAsync(int? regionId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select district_id as DistrictId, region_id as RegionId, district_name as DistrictName
from catalog.districts
where (@RegionId is null or region_id = @RegionId)
order by district_name";
        var rows = await connection.QueryAsync<DistrictResponse>(new CommandDefinition(sql, new { RegionId = regionId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ReferenceItemResponse>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "select category_id as Id, category_code as Code, category_name as Name from catalog.categories order by category_name";
        var rows = await connection.QueryAsync<ReferenceItemResponse>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}

public sealed class DashboardService : IDashboardService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DashboardService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<SellerDashboardSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select
    (select count(1)
     from marketplace.listings l
     inner join catalog.listing_statuses ls on ls.listing_status_id = l.listing_status_id
     where l.seller_user_id = @SellerUserId and ls.status_code = 'PUBLISHED') as ActiveListings,
    (select count(1) from messaging.enquiries e where e.seller_user_id = @SellerUserId) as ReceivedEnquiries,
    (select sp.plan_name
     from billing.seller_subscriptions ss
     inner join billing.subscription_plans sp on sp.plan_id = ss.plan_id
     where ss.seller_user_id = @SellerUserId and ss.end_date_utc >= now()
     order by ss.end_date_utc desc
     limit 1) as ActivePlanName,
    (select ss.end_date_utc
     from billing.seller_subscriptions ss
     where ss.seller_user_id = @SellerUserId and ss.end_date_utc >= now()
     order by ss.end_date_utc desc
     limit 1) as SubscriptionEndDateUtc";

        return await connection.QuerySingleAsync<SellerDashboardSummaryResponse>(new CommandDefinition(sql, new { SellerUserId = sellerUserId }, cancellationToken: cancellationToken));
    }
}
