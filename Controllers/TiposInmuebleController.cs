using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaULP.Controllers
{
    [Authorize]
    public class TiposInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble _repo;

        public TiposInmuebleController(IRepositorioTipoInmueble repo)
        {
            _repo = repo;
        }

        // GET: TiposInmueble
        public IActionResult Index(string search)
        {
            var items = _repo.ObtenerTodos();
            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(t => t.Descripcion.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Search = search;
            return View(items);
        }

        // GET: TiposInmueble/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var tipo = _repo.ObtenerPorId(id.Value);
            if (tipo == null) return NotFound();

            return View(tipo);
        }

        // GET: TiposInmueble/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TiposInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdTipoInmueble,Descripcion")] TipoInmueble tipoInmueble)
        {
            if (ModelState.IsValid)
            {
                _repo.Alta(tipoInmueble);
                TempData["Success"] = "Tipo de inmueble creado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }

        // GET: TiposInmueble/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var tipo = _repo.ObtenerPorId(id.Value);
            if (tipo == null) return NotFound();

            return View(tipo);
        }

        // POST: TiposInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdTipoInmueble,Descripcion")] TipoInmueble tipoInmueble)
        {
            if (id != tipoInmueble.IdTipoInmueble) return NotFound();

            if (ModelState.IsValid)
            {
                _repo.Modificacion(tipoInmueble);
                TempData["Success"] = "Tipo de inmueble actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }

        // GET: TiposInmueble/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var tipo = _repo.ObtenerPorId(id.Value);
            if (tipo == null) return NotFound();

            return View(tipo);
        }

        // POST: TiposInmueble/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.Baja(id);
                TempData["Success"] = "Tipo de inmueble eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se puede eliminar el tipo de inmueble porque existen propiedades asociadas a él.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}