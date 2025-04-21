using Module35.Models;
using System;

namespace Module35.ViewModels;

public class UserViewModel 
{
    public User User { get; }
  
    public UserViewModel(User user) 
    {
        User = user ?? throw new ArgumentNullException(nameof(user), "Пользователь не может быть пустым");
    }
} 