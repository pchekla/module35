using Module35;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Настройка конвейера HTTP запросов
startup.Configure(app, app.Environment);

app.Run();