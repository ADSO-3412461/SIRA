using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SIRA.Configuration;
using SIRA.Data;
using SIRA.Repositories.Implementations;
using SIRA.Repositories.Interfaces;
using SIRA.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Autenticación por cookie ──────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ── EF Core / SQLite ─────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SiraDb")));

// ── Repositorios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IExcusaRepository,       ExcusaRepository>();
builder.Services.AddScoped<IEstudianteRepository,   EstudianteRepository>();
builder.Services.AddScoped<IUsuarioRepository,      UsuarioRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();

// ── Email ─────────────────────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // debe ir antes de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
