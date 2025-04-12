using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Module35.Data;
using Module35.Models;
using System.Reflection;

namespace Module35;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // Метод для настройки сервисов приложения
    public void ConfigureServices(IServiceCollection services)
    {
        // Получаем строку подключения из конфигурации
        string? connectionString = Configuration.GetConnectionString("DefaultConnection");
        string connection = connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Добавляем контекст базы данных
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlServer(connection));

        // Добавляем Identity с настройками паролей
        services.AddIdentity<User, IdentityRole>(opts => {
                opts.Password.RequiredLength = 5;   
                opts.Password.RequireNonAlphanumeric = false;  
                opts.Password.RequireLowercase = false; 
                opts.Password.RequireUppercase = false; 
                opts.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Добавляем AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Добавляем контроллеры и представления
        services.AddControllersWithViews();
    }

    // Метод для настройки конвейера HTTP запросов
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection(); // Перенаправление на HTTPS только в production
        }

        app.UseStaticFiles();
        app.UseRouting();

        // Добавляем Authentication и Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
} 