using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class TiposInmuebleController : Controller
    {
        private readonly DataContext _context;

        public TiposInmuebleController(DataContext context)
        {
            _context = context;
        }

        // GET: TiposInmueble
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.TiposInmueble.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Descripcion.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Descripcion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: TiposInmueble/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tipo = await _context.TiposInmueble.FirstOrDefaultAsync(m => m.IdTipoInmueble == id);
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
        public async Task<IActionResult> Create([Bind("IdTipoInmueble,Descripcion")] TipoInmueble tipoInmueble)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.TiposInmueble.AnyAsync(t => t.Descripcion == tipoInmueble.Descripcion);
                if (existe)
                {
                    ModelState.AddModelError("Descripcion", "Ya existe un tipo de inmueble con esa descripcion.");
                    return View(tipoInmueble);
                }

                _context.Add(tipoInmueble);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tipo de inmueble creado con exito.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }

        // GET: TiposInmueble/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tipo = await _context.TiposInmueble.FindAsync(id);
            if (tipo == null) return NotFound();

            return View(tipo);
        }

        // POST: TiposInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTipoInmueble,Descripcion")] TipoInmueble tipoInmueble)
        {
            if (id != tipoInmueble.IdTipoInmueble) return NotFound();

            if (ModelState.IsValid)
            {
                bool existe = await _context.TiposInmueble.AnyAsync(t => t.Descripcion == tipoInmueble.Descripcion && t.IdTipoInmueble != id);
                if (existe)
                {
                    ModelState.AddModelError("Descripcion", "Ya existe otro tipo con esa descripcion.");
                    return View(tipoInmueble);
                }

                try
                {
                    _context.Update(tipoInmueble);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tipo de inmueble actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TiposInmueble.Any(e => e.IdTipoInmueble == tipoInmueble.IdTipoInmueble))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }

        // GET: TiposInmueble/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tipo = await _context.TiposInmueble.FirstOrDefaultAsync(m => m.IdTipoInmueble == id);
            if (tipo == null) return NotFound();

            return View(tipo);
        }

        // POST: TiposInmueble/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool enUso = await _context.Inmuebles.AnyAsync(i => i.IdTipoInmueble == id);
            if (enUso)
            {
                TempData["Error"] = "No se puede eliminar el tipo de inmueble porque existen propiedades asociadas.";
                return RedirectToAction(nameof(Index));
            }

            var tipo = await _context.TiposInmueble.FindAsync(id);
            if (tipo != null)
            {
                _context.TiposInmueble.Remove(tipo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tipo de inmueble eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
