using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly DataContext _context;

        public InquilinosController(DataContext context)
        {
            _context = context;
        }

        // GET: Inquilinos
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Inquilinos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Nombre.Contains(search) ||
                                         i.Apellido.Contains(search) ||
                                         i.Dni.Contains(search) ||
                                         i.Email.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = await _context.Inquilinos.FirstOrDefaultAsync(m => m.IdInquilino == id);
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
        public async Task<IActionResult> Create([Bind("IdInquilino,Dni,Nombre,Apellido,Telefono,Email,Estado")] Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                bool existeDni = await _context.Inquilinos.AnyAsync(i => i.Dni == inquilino.Dni);
                if (existeDni)
                {
                    ModelState.AddModelError("Dni", "Ya existe un inquilino registrado con este DNI.");
                    return View(inquilino);
                }

                _context.Add(inquilino);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Inquilino registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = await _context.Inquilinos.FindAsync(id);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInquilino,Dni,Nombre,Apellido,Telefono,Email,Estado")] Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeDni = await _context.Inquilinos.AnyAsync(i => i.Dni == inquilino.Dni && i.IdInquilino != id);
                if (existeDni)
                {
                    ModelState.AddModelError("Dni", "El DNI ingresado ya pertenece a otro inquilino.");
                    return View(inquilino);
                }

                try
                {
                    _context.Update(inquilino);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Inquilino actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inquilinos.Any(e => e.IdInquilino == inquilino.IdInquilino))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = await _context.Inquilinos.FirstOrDefaultAsync(m => m.IdInquilino == id);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inquilino = await _context.Inquilinos.FindAsync(id);
            if (inquilino != null)
            {
                _context.Inquilinos.Remove(inquilino);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Inquilino eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}