using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Messaging;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class MessagingService : IMessagingService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly INotificationService _notifications;

    public MessagingService(IDbConnectionFactory connectionFactory, INotificationService notifications)
    {
        _connectionFactory = connectionFactory;
        _notifications = notifications;
    }

    public async Task<ConversationResponse> StartConversationAsync(Guid enquiryId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string existsSql = "select conversation_id from messaging.conversations where enquiry_id = @EnquiryId limit 1";
        var existingId = await connection.ExecuteScalarAsync<Guid?>(new CommandDefinition(existsSql,
            new { EnquiryId = enquiryId }, cancellationToken: cancellationToken));
        if (existingId.HasValue)
            return (await GetConversationByIdAsync(existingId.Value, cancellationToken))!;

        const string enquirySql = "select buyer_user_id as BuyerUserId, seller_user_id as SellerUserId from messaging.enquiries where enquiry_id = @EnquiryId";
        var enquiry = await connection.QuerySingleOrDefaultAsync<(Guid BuyerUserId, Guid SellerUserId)>(
            new CommandDefinition(enquirySql, new { EnquiryId = enquiryId }, cancellationToken: cancellationToken));
        if (enquiry == default)
            throw new InvalidOperationException("Enquiry not found.");

        var convId = Guid.NewGuid();
        const string insertSql = @"
insert into messaging.conversations (conversation_id, enquiry_id, buyer_user_id, seller_user_id, created_at_utc)
values (@ConvId, @EnquiryId, @BuyerUserId, @SellerUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(insertSql, new
        {
            ConvId = convId,
            EnquiryId = enquiryId,
            enquiry.BuyerUserId,
            enquiry.SellerUserId
        }, cancellationToken: cancellationToken));

        return (await GetConversationByIdAsync(convId, cancellationToken))!;
    }

    public async Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select c.conversation_id as ConversationId, c.enquiry_id as EnquiryId,
       c.buyer_user_id as BuyerUserId, bu.full_name as BuyerName,
       c.seller_user_id as SellerUserId, su.full_name as SellerName,
       c.created_at_utc as CreatedAtUtc
from messaging.conversations c
inner join auth.users bu on bu.user_id = c.buyer_user_id
inner join auth.users su on su.user_id = c.seller_user_id
where c.buyer_user_id = @UserId or c.seller_user_id = @UserId
order by c.created_at_utc desc";
        var convs = (await connection.QueryAsync<ConversationRow>(new CommandDefinition(sql,
            new { UserId = userId }, cancellationToken: cancellationToken))).ToList();

        var result = new List<ConversationResponse>();
        foreach (var c in convs)
        {
            var lastMsg = await GetLastMessageAsync(connection, c.ConversationId, cancellationToken);
            result.Add(new ConversationResponse(c.ConversationId, c.EnquiryId, c.BuyerUserId, c.BuyerName,
                c.SellerUserId, c.SellerName, c.CreatedAtUtc, lastMsg));
        }
        return result;
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string partSql = "select count(1) from messaging.conversations where conversation_id = @ConvId and (buyer_user_id = @UserId or seller_user_id = @UserId)";
        var isParticipant = await connection.ExecuteScalarAsync<int>(new CommandDefinition(partSql,
            new { ConvId = conversationId, UserId = userId }, cancellationToken: cancellationToken));
        if (isParticipant == 0) throw new UnauthorizedAccessException("Access denied.");

        const string sql = @"
select m.message_id as MessageId, m.conversation_id as ConversationId,
       m.sender_user_id as SenderUserId, u.full_name as SenderName,
       m.message_body as MessageBody, m.is_read as IsRead, m.sent_at_utc as SentAtUtc
from messaging.messages m
inner join auth.users u on u.user_id = m.sender_user_id
where m.conversation_id = @ConvId
order by m.sent_at_utc asc";
        var rows = await connection.QueryAsync<MessageResponse>(new CommandDefinition(sql,
            new { ConvId = conversationId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<MessageResponse> SendMessageAsync(Guid conversationId, Guid senderUserId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string partSql = "select buyer_user_id as BuyerUserId, seller_user_id as SellerUserId from messaging.conversations where conversation_id = @ConvId and (buyer_user_id = @UserId or seller_user_id = @UserId)";
        var conv = await connection.QuerySingleOrDefaultAsync<(Guid BuyerUserId, Guid SellerUserId)>(
            new CommandDefinition(partSql, new { ConvId = conversationId, UserId = senderUserId }, cancellationToken: cancellationToken));
        if (conv == default) throw new UnauthorizedAccessException("Access denied.");

        var msgId = Guid.NewGuid();
        const string insertSql = @"
insert into messaging.messages (message_id, conversation_id, sender_user_id, message_body, sent_at_utc, is_read)
values (@MsgId, @ConvId, @SenderUserId, @Body, now(), false)";
        await connection.ExecuteAsync(new CommandDefinition(insertSql, new
        {
            MsgId = msgId,
            ConvId = conversationId,
            SenderUserId = senderUserId,
            Body = request.MessageBody
        }, cancellationToken: cancellationToken));

        var recipientId = conv.BuyerUserId == senderUserId ? conv.SellerUserId : conv.BuyerUserId;
        await _notifications.CreateNotificationAsync(recipientId, "New Message", "You have a new message.", cancellationToken);

        const string getSql = @"
select m.message_id as MessageId, m.conversation_id as ConversationId,
       m.sender_user_id as SenderUserId, u.full_name as SenderName,
       m.message_body as MessageBody, m.is_read as IsRead, m.sent_at_utc as SentAtUtc
from messaging.messages m
inner join auth.users u on u.user_id = m.sender_user_id
where m.message_id = @MsgId";
        return await connection.QuerySingleAsync<MessageResponse>(new CommandDefinition(getSql,
            new { MsgId = msgId }, cancellationToken: cancellationToken));
    }

    public async Task MarkConversationReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
update messaging.messages set is_read = true
where conversation_id = @ConvId and sender_user_id != @UserId and is_read = false";
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new { ConvId = conversationId, UserId = userId }, cancellationToken: cancellationToken));
    }

    private async Task<ConversationResponse?> GetConversationByIdAsync(Guid convId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
select c.conversation_id as ConversationId, c.enquiry_id as EnquiryId,
       c.buyer_user_id as BuyerUserId, bu.full_name as BuyerName,
       c.seller_user_id as SellerUserId, su.full_name as SellerName,
       c.created_at_utc as CreatedAtUtc
from messaging.conversations c
inner join auth.users bu on bu.user_id = c.buyer_user_id
inner join auth.users su on su.user_id = c.seller_user_id
where c.conversation_id = @ConvId";
        var row = await connection.QuerySingleOrDefaultAsync<ConversationRow>(new CommandDefinition(sql,
            new { ConvId = convId }, cancellationToken: cancellationToken));
        if (row is null) return null;
        var lastMsg = await GetLastMessageAsync(connection, convId, cancellationToken);
        return new ConversationResponse(row.ConversationId, row.EnquiryId, row.BuyerUserId, row.BuyerName,
            row.SellerUserId, row.SellerName, row.CreatedAtUtc, lastMsg);
    }

    private static async Task<MessageResponse?> GetLastMessageAsync(System.Data.IDbConnection connection, Guid convId, CancellationToken cancellationToken)
    {
        const string sql = @"
select m.message_id as MessageId, m.conversation_id as ConversationId,
       m.sender_user_id as SenderUserId, u.full_name as SenderName,
       m.message_body as MessageBody, m.is_read as IsRead, m.sent_at_utc as SentAtUtc
from messaging.messages m
inner join auth.users u on u.user_id = m.sender_user_id
where m.conversation_id = @ConvId
order by m.sent_at_utc desc limit 1";
        return await connection.QuerySingleOrDefaultAsync<MessageResponse>(new CommandDefinition(sql,
            new { ConvId = convId }, cancellationToken: cancellationToken));
    }

    private sealed record ConversationRow(Guid ConversationId, Guid? EnquiryId, Guid BuyerUserId, string BuyerName, Guid SellerUserId, string SellerName, DateTime CreatedAtUtc);
}
