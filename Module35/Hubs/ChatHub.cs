using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Module35.Hubs;

[Authorize] // Доступ только для авторизованных пользователей
public class ChatHub : Hub
{
    // Словарь для сопоставления UserId с ConnectionId
    // В реальном приложении лучше использовать более надежное хранилище (БД, кэш)
    private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();

    // Публичный статический метод для получения ConnectionId по UserId
    public static string? GetUserConnectionId(string userId)
    {
        UserConnections.TryGetValue(userId, out string? connectionId);
        return connectionId;
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier; // Получаем ID пользователя из контекста
        if (!string.IsNullOrEmpty(userId))
        {
            UserConnections[userId] = Context.ConnectionId;
            Console.WriteLine($"User {userId} connected with connection ID {Context.ConnectionId}");
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            if (UserConnections.ContainsKey(userId))
            {
                 // Проверяем, совпадает ли ConnectionId перед удалением
                 // (пользователь мог открыть другую вкладку)
                 if (UserConnections[userId] == Context.ConnectionId)
                 {
                     UserConnections.Remove(userId);
                     Console.WriteLine($"User {userId} disconnected.");
                 }
            }
        }
        return base.OnDisconnectedAsync(exception);
    }

    // Метод для уведомления о наборе текста
    public async Task NotifyTyping(string recipientUserId, bool isTyping)
    {
        var senderUserId = Context.UserIdentifier;
         if (string.IsNullOrEmpty(senderUserId) || string.IsNullOrEmpty(recipientUserId))
            return;
            
        // Отправляем уведомление о наборе текста получателю, если он онлайн
        if (UserConnections.TryGetValue(recipientUserId, out string? recipientConnectionId))
        {           
            await Clients.Client(recipientConnectionId).SendAsync("ReceiveTypingNotification", senderUserId, isTyping);
        }
    }
} 