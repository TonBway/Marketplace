using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Messaging;
using FarmMarketplace.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class LocalPushNotificationService : IPushNotificationService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<LocalPushNotificationService> _logger;

    public LocalPushNotificationService(IDbConnectionFactory connectionFactory, ILogger<LocalPushNotificationService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select token, platform from messaging.device_tokens
where user_id = @UserId and is_active = true";
        var tokens = await connection.QueryAsync<(string Token, string Platform)>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        foreach (var (token, platform) in tokens)
        {
            _logger.LogInformation("[DEV PUSH] Platform={Platform} Token={Token} Title={Title} Body={Body}",
                platform, token[..Math.Min(20, token.Length)] + "…", title, body);
        }
    }
}
