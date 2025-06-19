// Проект: MajorAuthor.Web
// Файл: Program.cs

using MajorAuthor.Data; // Используем наш DbContext
using Microsoft.EntityFrameworkCore; // Используем Entity Framework Core
using Microsoft.AspNetCore.Identity; // Если вы будете использовать ASP.NET Core Identity

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.

// Настройка строки подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Регистрация MajorAuthorDbContext
builder.Services.AddDbContext<MajorAuthorDbContext>(options =>
    options.UseSqlServer(connectionString)); // Используйте UseSqlite, UseNpgsql и т.д., если используете другую БД

// --- Опционально: Добавление ASP.NET Core Identity ---
// Если вы планируете использовать систему аутентификации и управления пользователями ASP.NET Core Identity,
// раскомментируйте следующие строки и настройте ее.
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MajorAuthorDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигурация конвейера HTTP-запросов.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- Автоматическое применение миграций и инициализация базы данных при запуске (ТОЛЬКО ДЛЯ РАЗРАБОТКИ!) ---
// Этот блок кода очень полезен в процессе разработки для автоматического обновления и заполнения базы данных.
// В production-среде миграции следует применять контролируемо, например, с помощью CI/CD пайплайна,
// а инициализацию данных - отдельно, если это необходимо.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MajorAuthorDbContext>();
        context.Database.Migrate(); // Применяет все ожидающие миграции
        Console.WriteLine("Database migrations applied successfully.");

        // Инициализация базы данных (seeding)
        await DbInitializer.Initialize(context); // Вызов метода инициализации
        Console.WriteLine("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// --------------------------------------------------------------------------------------

// app.UseAuthentication(); // Если используете ASP.NET Core Identity (раскомментируйте после настройки)
// app.UseAuthorization();  // Если используете ASP.NET Core Identity (раскомментируйте после настройки)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
