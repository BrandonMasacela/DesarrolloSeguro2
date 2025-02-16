using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Prestamo.Data;

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
// Registrar CuentaData con la cadena de conexión
builder.Services.AddSingleton<CuentaData>(provider =>
{
    var connectionStrings = provider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
    return new CuentaData(connectionStrings.CadenaSQL);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Login/Index";
        option.LogoutPath = "/Login/Salir";
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
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
