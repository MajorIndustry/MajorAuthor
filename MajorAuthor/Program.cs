// Проект: MajorAuthor.Web
// Файл: Program.cs

using MajorAuthor.Data; // Используем наш DbContext
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services; // Если вы будете использовать ASP.NET Core Identity
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Web.DependencyInjection; // Используем Entity Framework Core

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.

// Настройка строки подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Регистрация MajorAuthorDbContext
//builder.Services.AddDbContext<MajorAuthorDbContext>(options =>
//    options.UseSqlServer(connectionString)); // Используйте UseSqlite, UseNpgsql и т.д., если используете другую БД
builder.Services.AddDbContextFactory<MajorAuthorDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

//    .AddEntityFrameworkStores<MajorAuthorDbContext>();
// Настройка ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MajorAuthorDbContext>(); // Использование MajorAuthorDbContext
// Конфигурация для HTTP: МАКСИМАЛЬНОЕ ПОНИЖЕНИЕ БЕЗОПАСНОСТИ
// Это позволит работать без HTTPS, но НЕ РЕКОМЕНДУЕТСЯ.
// 1. Конфигурируем основной Identity Cookie, отключая Secure
builder.Services.ConfigureApplicationCookie(options =>
{
    // Отключаем требование HTTPS для основного cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    // Явно указываем, что это не межсайтовый запрос (Lax)
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 2. Конфигурируем внешний Identity Cookie (ExternalScheme)
//builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
//{
//    // Отключаем требование HTTPS
//    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
//    // Устанавливаем Unspecified, чтобы браузер не блокировал его при возврате
//    options.Cookie.SameSite = SameSiteMode.Unspecified;
//});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Устанавливает время, в течение которого данные внешнего логина 
    // (например, email, который нужно подтвердить) остаются действительными.
    // По умолчанию очень мало (5 минут). Увеличим до 30 минут.
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});
builder.Services.ConfigureExternalCookie(options =>
{
    // Установим срок жизни временной куки внешнего входа 
    // (та, которая содержит info о логине)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
// --- Начало изменений для внешних провайдеров ---
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // Получение Client ID и Client Secret из конфигурации
        // Для разработки используйте User Secrets: dotnet user-secrets set "Authentication:Google:ClientId" "ВАШ_CLIENT_ID"
        // dotnet user-secrets set "Authentication:Google:ClientSecret" "ВАШ_CLIENT_SECRET"
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddYandex(yandexOptions =>
    {
        // Для разработки используйте User Secrets: dotnet user-secrets set "Authentication:Yandex:ClientId" "ВАШ_APP_ID"
        // dotnet user-secrets set "Authentication:Yandex:ClientSecret" "ВАШ_APP_SECRET"
        yandexOptions.ClientId = builder.Configuration["Authentication:Yandex:ClientId"];
        yandexOptions.ClientSecret = builder.Configuration["Authentication:Yandex:ClientSecret"];
        // 💡 НОВОЕ: Добавляем параметр для принудительного выбора аккаунта
        yandexOptions.Scope.Add("login:info"); // Предполагается, что это уже есть
        yandexOptions.Scope.Add("login:email"); // Предполагается, что это уже есть

        //yandexOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
        //{
        //    // Добавляем параметр prompt=select_account (или login=yes, или force_auth=true)
        //    // Яндекс часто использует force_auth=true или редирект с параметром
        //    var separator = context.RedirectUri.Contains('?') ? "&" : "?";

        //    // В зависимости от версии API Яндекса, может потребоваться 'force_auth=yes'
        //    context.RedirectUri = context.RedirectUri + separator + "force_auth=yes";

        //    context.Response.Redirect(context.RedirectUri);
        //    return Task.CompletedTask;
        //};
    });
// --- Конец изменений для внешних провайдеров ---

// === НАСТРОЙКИ И РЕГИСТРАЦИЯ СЛУЖБЫ EMAIL ===
// Привязываем секцию "EmailSettings" из конфигурации к классу EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Регистрируем нашу службу отправки электронной почты
builder.Services.AddTransient<IEmailSender, EmailSender>(); // Регистрируем как Transient
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<IHomeService, HomeServiceWithFactory>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IWorkService<Poem>, PoemService>();
builder.Services.AddScoped<IWorkService<Blog>, BlogService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookInvitationService, BookInvitationService>();
builder.Services.AddScoped<IWorkFacade, WorkFacade>();
builder.Services.AddScoped<ICommentService, CommentService>();
// В файле Program.cs или Startup.cs добавьте:
builder.Services.AddScoped<IMessageService, MessageService>();
// Добавить в контейнер зависимостей
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
// Регистрация сервиса уведомлений
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddHostedService<SimilarBookCalculatorService>();
builder.Services.AddScoped<ISimilarBookCalculatorService, SimilarBookCalculatorService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddSingleton<IPdfCacheService, PdfCacheService>();
// Добавить в контейнер зависимостей
builder.Services.AddScoped<IRatingManagerService, RatingManagerService>();
// Регистрация фонового сервиса
builder.Services.AddHostedService<RatingRecalculationService>();
builder.Services.AddScoped<IReadingProgressService, ReadingProgressService>();
builder.Services.AddSignalR(options =>
{
    // Увеличиваем лимит до 512 КБ (или больше, если книги огромные)
    options.MaximumReceiveMessageSize = 100 * 1024*1024; // 512 KB
    options.EnableDetailedErrors = true;
});
// ===========================================
builder.Services.AddImageSharp();
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Конфигурация конвейера HTTP-запросов.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.MapHub<MajorAuthor.Hubs.CollaborativeChapterHub>("/collaborativeChapterHub");
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
 app.UseAuthorization();  // Если используете ASP.NET Core Identity (раскомментируйте после настройки)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
