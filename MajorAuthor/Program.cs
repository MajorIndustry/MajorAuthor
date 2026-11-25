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

using Microsoft.EntityFrameworkCore; // Используем Entity Framework Core

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
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
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
var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
    });
}

var yandexClientId = builder.Configuration["Authentication:Yandex:ClientId"];
var yandexClientSecret = builder.Configuration["Authentication:Yandex:ClientSecret"];
if (!string.IsNullOrEmpty(yandexClientId) && !string.IsNullOrEmpty(yandexClientSecret))
{
    authBuilder.AddYandex(yandexOptions =>
    {
        yandexOptions.ClientId = yandexClientId;
        yandexOptions.ClientSecret = yandexClientSecret;
        yandexOptions.Scope.Add("login:info");
        yandexOptions.Scope.Add("login:email");
    });
}
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
    options.MaximumReceiveMessageSize = 1 * 1024 * 1024; // 1 MB
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
// ===========================================
builder.Services.AddHealthChecks();
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

app.UseAuthentication();
app.UseAuthorization();

// --- Автоматическое применение миграций и инициализация базы данных при запуске (ТОЛЬКО ДЛЯ РАЗРАБОТКИ или при Database:AutoMigrate=true) ---
var autoMigrate = app.Environment.IsDevelopment() ||
                  app.Configuration.GetValue<bool>("Database:AutoMigrate");

if (autoMigrate)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var context = services.GetRequiredService<MajorAuthorDbContext>();
            context.Database.Migrate(); // Применяет все ожидающие миграции
            logger.LogInformation("Database migrations applied successfully.");

            // Инициализация базы данных (seeding)
            await DbInitializer.Initialize(context); // Вызов метода инициализации
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}
// --------------------------------------------------------------------------------------

app.MapHealthChecks("/health");
app.MapHub<MajorAuthor.Hubs.CollaborativeChapterHub>("/collaborativeChapterHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
