using Module35;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// --- Начало блока инициализации БД ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Module35.Data.ApplicationDbContext>();
        var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Module35.Models.User>>();
        var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        // Удаляем базу данных, если она существует
        await context.Database.EnsureDeletedAsync();
        Console.WriteLine("Database deleted successfully.");

        // Создаем базу данных заново на основе моделей EF Core
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database created successfully.");

        // Создаем 20 пользователей
        string baseUserName = "User";
        string password = "Password123!"; // Пароль из README
        string defaultAvatarPath = "https://cvam.ru/wp-content/uploads/2023/10/frai-shat-ap-1.webp"; // Новая ссылка на аватар

        for (int i = 0; i < 20; i++)
        {
            string userName = $"{baseUserName}{i}";
            string email = $"{userName}@example.com"; // Генерируем email

            var user = new Module35.Models.User
            {
                UserName = userName,
                Email = email,
                FirstName = $"Имя{i}", // Добавляем базовые данные
                LastName = $"Фамилия{i}",
                About = $"О себе пользователя {i}",
                BirthDate = new DateTime(1990 + (i % 20), (i % 12) + 1, (i % 28) + 1) // Пример даты рождения
            };

            // Устанавливаем аватар для User10 - User19
            if (i >= 10)
            {
                user.Image = defaultAvatarPath;
            }

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                Console.WriteLine($"User {userName} created successfully.");
            }
            else
            {
                Console.WriteLine($"Error creating user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the DB.");
    }
}
// --- Конец блока инициализации БД ---

// Настройка конвейера HTTP запросов
startup.Configure(app, app.Environment);

app.Run();