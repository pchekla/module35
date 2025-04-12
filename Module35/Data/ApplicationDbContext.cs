using Microsoft.EntityFrameworkCore;
using Module35.Models;

namespace Module35.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // Здесь можно добавить DbSet свойства для ваших моделей
    // Пример: public DbSet<YourModel> YourModels { get; set; }
} 