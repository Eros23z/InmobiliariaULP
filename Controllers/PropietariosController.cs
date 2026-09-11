using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaULP.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repo;

        public PropietariosController(IRepositorioPropietario repo)
        {
            _repo = repo;
        }

        public IActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            var items = _repo.ObtenerTodos(search, page, pageSize);
            int total = _repo.Contar(search);

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(items);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var p = _repo.ObtenerPorId(id.Value);
            if (p == null) return NotFound();
            return View(p);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Dni,Nombre,Apellido,Telefono,Email,Estado")] Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                _repo.Alta(propietario);
                TempData["Success"] = "Propietario registrado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();
            var p = _repo.ObtenerPorId(id.Value);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdPropietario,Dni,Nombre,Apellido,Telefono,Email,Estado")] Propietario propietario)
        {
            if (id != propietario.IdPropietario) return NotFound();

            if (ModelState.IsValid)
            {
                _repo.Modificacion(propietario);
                TempData["Success"] = "Propietario actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            var p = _repo.ObtenerPorId(id.Value);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Baja(id);
            TempData["Success"] = "Propietario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}