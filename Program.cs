using InmobiliariaULP.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRepositorioPropietario, RepositorioPropietario>();
builder.Services.AddScoped<IRepositorioInquilino, RepositorioInquilino>();
builder.Services.AddScoped<IRepositorioTipoInmueble, RepositorioTipoInmueble>();
builder.Services.AddScoped<IRepositorioInmueble, RepositorioInmueble>();
builder.Services.AddScoped<IRepositorioReserva, RepositorioReserva>();
builder.Services.AddScoped<IRepositorioPago, RepositorioPago>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioReporte, RepositorioReporte>();
builder.Services.AddScoped<IRepositorioImagenInmueble, RepositorioImagenInmueble>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Home/Restringido";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inmuebles}/{action=Index}/{id?}")
    .WithStaticAssets();

// utilizo este bloque para generar los usuarios con contraseña hasheada

using (var scope = app.Services.CreateScope())
{
    var repoUsuario = scope.ServiceProvider.GetRequiredService<IRepositorioUsuario>();

    if (repoUsuario.ObtenerPorEmail("admin@inmobiliaria.com") == null)
    {
        repoUsuario.Alta(new InmobiliariaULP.Models.Usuario
        {
            Nombre = "Administrador",
            Apellido = "General",
            Email = "admin@inmobiliaria.com",
            Clave = "admin123",
            Rol = "Administrador",
            Estado = true
        });
    }

    if (repoUsuario.ObtenerPorEmail("empleado@inmobiliaria.com") == null)
    {
        repoUsuario.Alta(new InmobiliariaULP.Models.Usuario
        {
            Nombre = "Juan",
            Apellido = "Empleado",
            Email = "empleado@inmobiliaria.com",
            Clave = "empleado123",
            Rol = "Empleado",
            Estado = true
        });
    }
}

app.Run();
