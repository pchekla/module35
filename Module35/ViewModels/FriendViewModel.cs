using Module35.Models;

namespace Module35.ViewModels;

public class FriendViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public bool IsRequestPending { get; set; } = false;
    public int? RelationId { get; set; }
    
    public FriendViewModel() {}
    
    public FriendViewModel(User user, FriendRelation relation = null)
    {
        if (user == null) return;
        
        Id = user.Id ?? string.Empty;
        FullName = string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName) 
            ? "Пользователь" 
            : user.GetFullName();
        Image = user.Image ?? string.Empty;
        BirthDate = (user.BirthDate.Year > 1900) ? user.BirthDate : null;
        
        if (relation != null)
        {
            IsRequestPending = !relation.IsAccepted;
            RelationId = relation.Id;
        }
    }
} 