using Microsoft.AspNetCore.Mvc;
using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;

namespace InmobiliariaULP.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repo;

        public InquilinosController(IRepositorioInquilino repo)
        {
            _repo = repo;
        }

        // GET: Inquilinos
        public IActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            var items = _repo.ObtenerTodos(search, page, pageSize);
            int total = _repo.Contar(search);

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(items);
        }

        // GET: Inquilinos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = _repo.ObtenerPorId(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdInquilino,Dni,Nombre,Apellido,Telefono,Email,Estado")] Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _repo.Alta(inquilino);
                    TempData["Success"] = "Inquilino registrado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Dni", "Error al guardar el inquilino o el DNI ya se encuentra registrado.");
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = _repo.ObtenerPorId(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdInquilino,Dni,Nombre,Apellido,Telefono,Email,Estado")] Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _repo.Modificacion(inquilino);
                    TempData["Success"] = "Inquilino actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Dni", "El DNI ingresado ya pertenece a otro inquilino.");
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = _repo.ObtenerPorId(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Baja(id);
            TempData["Success"] = "Inquilino eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}