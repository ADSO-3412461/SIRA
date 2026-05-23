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

// ── Session ───────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout      = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly  = true;
    options.Cookie.IsEssential = true;
});

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
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "sira.db");
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

// ── Repositorios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IExcusaRepository,        ExcusaRepository>();
builder.Services.AddScoped<IEstudianteRepository,    EstudianteRepository>();
builder.Services.AddScoped<IUsuarioRepository,       UsuarioRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IAcudienteRepository,             AcudienteRepository>();
builder.Services.AddScoped<ITipoDocumentoRepository,         TipoDocumentoRepository>();
builder.Services.AddScoped<IInstitucionEducativaRepository,  InstitucionEducativaRepository>();
builder.Services.AddScoped<IAuditoriaRepository,             AuditoriaRepository>();

// ── Email ─────────────────────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseDeveloperExceptionPage();
// ── Pipeline ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // debe ir antes de UseAuthorization
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
