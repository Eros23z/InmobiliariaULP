using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InmobiliariaULP.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly IRepositorioInmueble _repoInmueble;
        private readonly IRepositorioPropietario _repoPropietario;
        private readonly IRepositorioTipoInmueble _repoTipoInmueble;

        public InmueblesController(
            IRepositorioInmueble repoInmueble,
            IRepositorioPropietario repoPropietario,
            IRepositorioTipoInmueble repoTipoInmueble)
        {
            _repoInmueble = repoInmueble;
            _repoPropietario = repoPropietario;
            _repoTipoInmueble = repoTipoInmueble;
        }

        private void CargarListasDesplegables(int? idPropietario = null, int? idTipo = null)
        {
            var propietarios = _repoPropietario.ObtenerTodos(null, 1, 500)
                .Where(p => p.Estado)
                .OrderBy(p => p.Apellido);

            var tipos = _repoTipoInmueble.ObtenerTodos()
                .OrderBy(t => t.Descripcion);

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "NombreCompleto", idPropietario);
            ViewBag.IdTipoInmueble = new SelectList(tipos, "IdTipoInmueble", "Descripcion", idTipo);
        }

        // GET: Inmuebles
        public IActionResult Index(string search, int? tipoId, bool? soloDisponibles, int page = 1, int pageSize = 10)
        {
            var items = _repoInmueble.ObtenerTodos(search, tipoId, soloDisponibles);

            int totalItems = items.Count;
            var paginados = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TipoId = tipoId;
            ViewBag.SoloDisponibles = soloDisponibles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TiposFiltro = new SelectList(_repoTipoInmueble.ObtenerTodos(), "IdTipoInmueble", "Descripcion", tipoId);

            return View(paginados);
        }

        // GET: Inmuebles/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _repoInmueble.Alta(inmueble);
                TempData["Success"] = "Inmueble publicado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble)
        {
            if (id != inmueble.IdInmueble) return NotFound();

            if (ModelState.IsValid)
            {
                _repoInmueble.Modificacion(inmueble);
                TempData["Success"] = "Inmueble actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repoInmueble.Baja(id);
                TempData["Success"] = "Inmueble eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se puede eliminar el inmueble porque registra reservas asociadas. Se recomienda suspender su disponibilidad.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Endpoint AJAX
        [HttpGet]
        public IActionResult ObtenerPrecio(int id)
        {
            var inmueble = _repoInmueble.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return Json(new { precio = inmueble.PrecioPorDia, porcentaje = inmueble.PorcentajeReserva });
        }
    }
}