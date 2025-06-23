// ������: MajorAuthor.Web
// ����: Program.cs

using MajorAuthor.Data; // ���������� ��� DbContext
using Microsoft.EntityFrameworkCore; // ���������� Entity Framework Core
using Microsoft.AspNetCore.Identity; // ���� �� ������ ������������ ASP.NET Core Identity

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� � ���������.

// ��������� ������ ����������� �� appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ����������� MajorAuthorDbContext
builder.Services.AddDbContext<MajorAuthorDbContext>(options =>
    options.UseSqlServer(connectionString)); // ����������� UseSqlite, UseNpgsql � �.�., ���� ����������� ������ ��

// --- �����������: ���������� ASP.NET Core Identity ---
// ���� �� ���������� ������������ ������� �������������� � ���������� �������������� ASP.NET Core Identity,
// ���������������� ��������� ������ � ��������� ��.
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MajorAuthorDbContext>();

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
// app.UseAuthorization();  // ���� ����������� ASP.NET Core Identity (���������������� ����� ���������)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
