using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;

namespace InmobiliariaULP.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;

        public UsuariosController(IRepositorioUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Inmuebles");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string clave, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave))
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar el correo y la contraseña.");
                return View();
            }

            var usuario = _repoUsuario.ObtenerPorEmail(email);

            if (usuario == null || usuario.Clave != clave)
            {
                ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario inactivo.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Inmuebles");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }


        public IActionResult Perfil()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int id)) return RedirectToAction(nameof(Login));

            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModificarPerfil(Usuario usuario)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int id) || id != usuario.IdUsuario) return Forbid();

            if (ModelState.IsValid)
            {
                _repoUsuario.ModificarPerfil(usuario);
                TempData["Success"] = "Datos personales actualizados con éxito.";
                return RedirectToAction(nameof(Perfil));
            }

            return View("Perfil", usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(int idUsuario, string claveActual, string nuevaClave, string confirmarClave)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int id) || id != idUsuario) return Forbid();

            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            if (usuario.Clave != claveActual)
            {
                TempData["Error"] = "La contraseña actual es incorrecta.";
                return RedirectToAction(nameof(Perfil));
            }

            if (string.IsNullOrWhiteSpace(nuevaClave) || nuevaClave != confirmarClave)
            {
                TempData["Error"] = "Las nuevas contraseñas no coinciden o están vacías.";
                return RedirectToAction(nameof(Perfil));
            }

            _repoUsuario.ModificarClave(id, nuevaClave);
            TempData["Success"] = "Contraseña modificada correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // gestion de usuarios

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            var usuarios = _repoUsuario.ObtenerTodos();
            return View(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (_repoUsuario.ObtenerPorEmail(usuario.Email) != null)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con este correo electrónico.");
                    return View(usuario);
                }

                _repoUsuario.Alta(usuario);
                TempData["Success"] = "Usuario creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = _repoUsuario.ObtenerPorId(id.Value);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                _repoUsuario.Modificacion(usuario);
                TempData["Success"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            var usuario = _repoUsuario.ObtenerPorId(id.Value);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repoUsuario.Baja(id);
            TempData["Success"] = "Usuario dado de baja exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}