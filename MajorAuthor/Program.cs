// ������: MajorAuthor.Web
// ����: Program.cs

using MajorAuthor.Data; // ���������� ��� DbContext
using Microsoft.EntityFrameworkCore; // ���������� Entity Framework Core
using Microsoft.AspNetCore.Identity;
using MajorAuthor.Models;
using MajorAuthor.Services; // ���� �� ������ ������������ ASP.NET Core Identity

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� � ���������.

// ��������� ������ ����������� �� appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ����������� MajorAuthorDbContext
builder.Services.AddDbContext<MajorAuthorDbContext>(options =>
    options.UseSqlServer(connectionString)); // ����������� UseSqlite, UseNpgsql � �.�., ���� ����������� ������ ��

//    .AddEntityFrameworkStores<MajorAuthorDbContext>();
// ��������� ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MajorAuthorDbContext>(); // ������������� MajorAuthorDbContext

// --- ������ ��������� ��� ������� ����������� ---
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // ��������� Client ID � Client Secret �� ������������
        // ��� ���������� ����������� User Secrets: dotnet user-secrets set "Authentication:Google:ClientId" "���_CLIENT_ID"
        // dotnet user-secrets set "Authentication:Google:ClientSecret" "���_CLIENT_SECRET"
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddYandex(yandexOptions =>
    {
        // ��� ���������� ����������� User Secrets: dotnet user-secrets set "Authentication:Yandex:ClientId" "���_APP_ID"
        // dotnet user-secrets set "Authentication:Yandex:ClientSecret" "���_APP_SECRET"
        yandexOptions.ClientId = builder.Configuration["Authentication:Yandex:ClientId"];
        yandexOptions.ClientSecret = builder.Configuration["Authentication:Yandex:ClientSecret"];
    });
// --- ����� ��������� ��� ������� ����������� ---

// === ��������� � ����������� ������ EMAIL ===
// ����������� ������ "EmailSettings" �� ������������ � ������ EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// ������������ ���� ������ �������� ����������� �����
builder.Services.AddTransient<IEmailSender, EmailSender>(); // ������������ ��� Transient
// ===========================================

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ������������ ��������� HTTP-��������.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- �������������� ���������� �������� � ������������� ���� ������ ��� ������� (������ ��� ����������!) ---
// ���� ���� ���� ����� ������� � �������� ���������� ��� ��������������� ���������� � ���������� ���� ������.
// � production-����� �������� ������� ��������� �������������, ��������, � ������� CI/CD ���������,
// � ������������� ������ - ��������, ���� ��� ����������.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MajorAuthorDbContext>();
        context.Database.Migrate(); // ��������� ��� ��������� ��������
        Console.WriteLine("Database migrations applied successfully.");

        // ������������� ���� ������ (seeding)
        await DbInitializer.Initialize(context); // ����� ������ �������������
        Console.WriteLine("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// --------------------------------------------------------------------------------------

// app.UseAuthentication(); // ���� ����������� ASP.NET Core Identity (���������������� ����� ���������)
 app.UseAuthorization();  // ���� ����������� ASP.NET Core Identity (���������������� ����� ���������)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
