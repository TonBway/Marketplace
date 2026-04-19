using System.Text;
using FarmMarketplace.Api.Middleware;
using FarmMarketplace.Api.Options;
using FarmMarketplace.Api.Services;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Infrastructure.Data;
using FarmMarketplace.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Bootstrap logger so any startup exception is visible immediately
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUpload"));
    builder.Services.Configure<LocalFileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FarmMarketplace";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FarmMarketplaceClients";
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_THIS_IN_ENVIRONMENT_VARIABLES";

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ISellerProfileService, SellerProfileService>();
    builder.Services.AddScoped<IBuyerProfileService, BuyerProfileService>();
    builder.Services.AddScoped<IListingService, ListingService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IEnquiryService, EnquiryService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IAdminPortalService, AdminPortalService>();
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
    builder.Services.AddScoped<IPushNotificationService, LocalPushNotificationService>();
    builder.Services.AddScoped<IShippingService, ShippingService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IMessagingService, MessagingService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

    var app = builder.Build();

    Log.Information("Content root: {ContentRoot}", app.Environment.ContentRootPath);
    Log.Information("Web root:     {WebRoot}", app.Environment.WebRootPath ?? "(none)");
    Log.Information("Environment:  {Env}", app.Environment.EnvironmentName);

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Only redirect to HTTPS when the app is actually configured to listen on HTTPS
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}
