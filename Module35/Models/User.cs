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
    public string Image { get; set; } = "";
    public string Status { get; set; } = "Ура! Я в соцсети!";
    public string About { get; set; } = "Информация обо мне.";
    
    public string GetFullName() 
    {
        // Проверки на null для всех компонентов имени
        string firstName = FirstName ?? string.Empty;
        string middleName = MiddleName ?? string.Empty;
        string lastName = LastName ?? string.Empty;
        
        string fullName = firstName;
        
        if (!string.IsNullOrWhiteSpace(middleName))
            fullName += " " + middleName;
            
        if (!string.IsNullOrWhiteSpace(lastName))
            fullName += " " + lastName;
            
        return string.IsNullOrWhiteSpace(fullName) ? "Пользователь" : fullName;
    }
} 