using System.ComponentModel.DataAnnotations;

namespace Module35.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email или логин")]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
        
    [Display(Name = "Запомнить?")]
    public bool RememberMe { get; set; }
} 