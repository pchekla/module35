using System.ComponentModel.DataAnnotations;

namespace Module35.ViewModels;

public class UserEditViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Введите имя")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Введите фамилию")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;
    
    [Display(Name = "Отчество")]
    public string MiddleName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    
    [Display(Name = "Изображение профиля")]
    [Url(ErrorMessage = "Введите корректный URL")]
    public string Image { get; set; } = "https://via.placeholder.com/500";
    
    [Display(Name = "Статус")]
    public string Status { get; set; } = "Ура! Я в соцсети!";
    
    [Display(Name = "О себе")]
    public string About { get; set; } = "Информация обо мне.";
} 