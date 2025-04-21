using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Module35.Models;

public class User : IdentityUser
{
    // Персональные данные пользователя
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    
    // Дата рождения
    public DateTime BirthDate { get; set; }
    
    // Свойства пользователя, теперь сохраняются в БД
    public string Image { get; set; } = "https://via.placeholder.com/500";
    public string Status { get; set; } = "Ура! Я в соцсети!";
    public string About { get; set; } = "Информация обо мне.";
    
    public string GetFullName() 
    {
        string fullName = FirstName;
        
        if (!string.IsNullOrWhiteSpace(MiddleName))
            fullName += " " + MiddleName;
            
        if (!string.IsNullOrWhiteSpace(LastName))
            fullName += " " + LastName;
            
        return string.IsNullOrWhiteSpace(fullName) ? "Пользователь" : fullName;
    }
} 