using System.ComponentModel.DataAnnotations;
using Module35.Models;

namespace Module35.ViewModels;

public class MessageViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Текст сообщения обязателен")]
    [MinLength(1, ErrorMessage = "Сообщение не может быть пустым")]
    [Display(Name = "Сообщение")]
    public string Text { get; set; } = string.Empty;
    
    public DateTime SentDate { get; set; } = DateTime.Now;
    
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderImage { get; set; } = string.Empty;
    
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    
    public bool IsFromCurrentUser { get; set; }
    
    public MessageViewModel() { }
    
    public MessageViewModel(Message message, string? currentUserId)
    {
        Id = message.Id;
        Text = message.Text ?? string.Empty;
        SentDate = message.SentDate;
        
        SenderId = message.SenderId;
        SenderName = message.Sender?.GetFullName() ?? "Пользователь";
        SenderImage = message.Sender?.Image ?? string.Empty;
        
        RecipientId = message.RecipientId;
        RecipientName = message.Recipient?.GetFullName() ?? "Получатель";
        
        IsFromCurrentUser = message.SenderId == currentUserId;
    }
} 