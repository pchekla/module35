using Module35.Models;

namespace Module35.ViewModels;

public class ChatViewModel
{
    public User? CurrentUser { get; set; }
    public User? Friend { get; set; }
    
    public string CurrentUserId { get; set; } = string.Empty;
    public string FriendId { get; set; } = string.Empty;
    
    public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
    public MessageViewModel NewMessage { get; set; }
    
    public ChatViewModel()
    {
        NewMessage = new MessageViewModel();
    }
    
    public ChatViewModel(User currentUser, User friend, List<Message> messages)
    {
        CurrentUser = currentUser;
        Friend = friend;
        
        CurrentUserId = currentUser?.Id ?? string.Empty;
        FriendId = friend?.Id ?? string.Empty;
        
        Messages = messages
            .OrderBy(m => m.SentDate)
            .Select(m => new MessageViewModel(m, currentUser?.Id))
            .ToList();
        
        NewMessage = new MessageViewModel 
        { 
            SenderId = CurrentUserId,
            RecipientId = FriendId
        };
    }
} 