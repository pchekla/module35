using System;
using Microsoft.EntityFrameworkCore;

namespace Module35.Models
{
    // DTO класс для безопасного получения результатов SQL запроса
    [Keyless]
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
} 