using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prestamo.Data;
using Prestamo.Web.Servives;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddSingleton<EmailService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<MonedaData>();
builder.Services.AddSingleton<ClienteData>();
builder.Services.AddSingleton<PrestamoData>();
builder.Services.AddSingleton<ResumenData>();
builder.Services.AddSingleton<UsuarioData>();
builder.Services.AddScoped<ResumenClienteData>();
builder.Services.AddSingleton<AuditoriaData>();
builder.Services.AddScoped<AuditoriaService>();
// Registrar CuentaData con la cadena de conexión
builder.Services.AddSingleton<CuentaData>();

// Configurar autenticación combinada (Cookies + JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Esquema principal para MVC
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
// Autenticación por cookies para el sitio web MVC
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login"; // Ruta de login
    options.AccessDeniedPath = "/Login"; // Ruta de acceso denegado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Tiempo de expiración
    options.Cookie.Name = "MiCookieAuth";
    options.SlidingExpiration = true; // Renovación automática
})
// Autenticación JWT para APIs
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context => {
            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configurar sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();


app.MapControllerRoute(
    //name: "default",
    //pattern: "{controller=Login}/{action=Index}/{id?}");
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
