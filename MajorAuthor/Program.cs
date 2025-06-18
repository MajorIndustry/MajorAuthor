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
// раскомментируйте следующие строки и настройте ее. В наших моделях User уже есть поле PasswordHash,
// но Identity предоставляет гораздо более полную систему (роли, токены, управление паролями).
// Если вы используете собственный класс User, то:
// builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<MajorAuthorDbContext>();
// Или, если вы хотите использовать стандартный IdentityUser и IdentityDbContext:
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MajorAuthorDbContext>();
// ВАЖНО: Если вы решите использовать ASP.NET Core Identity, вам потребуется создать
// отдельный IdentityDbContext или интегрировать Identity со своим MajorAuthorDbContext,
// что повлечет дополнительные миграции для таблиц Identity.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигурация конвейера HTTP-запросов.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Значение по умолчанию для HSTS production scenarios, см. https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- Опционально: Автоматическое применение миграций при запуске (ТОЛЬКО ДЛЯ РАЗРАБОТКИ!) ---
// Этот блок кода может быть полезен в процессе разработки для автоматического обновления базы данных.
// В production-среде миграции следует применять контролируемо, например, с помощью CI/CD пайплайна.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MajorAuthorDbContext>();
        context.Database.Migrate(); // Применяет все ожидающие миграции
        Console.WriteLine("Database migrations applied successfully.");

        // Здесь также можно добавить логику для инициализации данных (seeding)
        // DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// --------------------------------------------------------------------------------------

// app.UseAuthentication(); // Если используете ASP.NET Core Identity
// app.UseAuthorization();  // Если используете ASP.NET Core Identity

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
