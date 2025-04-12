using Microsoft.AspNetCore.Identity;

namespace Module35.Models;

public class User : IdentityUser
{
    // Персональные данные пользователя
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    
    // Дата рождения
    public DateTime BirthDate { get; set; }
} 