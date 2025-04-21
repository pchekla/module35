using Module35.Models;
using System;

namespace Module35.ViewModels;

public class UserViewModel 
{
    public User User { get; }
    
    // Поля для отображения статуса дружбы
    public bool IsFriend { get; set; } = false;
    public bool IsPendingRequest { get; set; } = false;
    public bool FriendRequestSentByMe { get; set; } = false;
    public int? RelationId { get; set; }
    
    // Флаг, определяющий, что это профиль самого пользователя
    public bool IsOwnProfile { get; set; } = false;
  
    public UserViewModel(User user) 
    {
        User = user ?? throw new ArgumentNullException(nameof(user), "Пользователь не может быть пустым");
    }
} 