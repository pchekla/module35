using Microsoft.AspNetCore.Identity;

namespace Module35.Models;

public class User : IdentityUser
{
    // Персональные данные пользователя
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    
    // Дата рождения
    public DateTime BirthDate { get; set; }
} 