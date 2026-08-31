using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly DataContext _context;

        public PropietariosController(DataContext context)
        {
            _context = context;
        }

        // GET: Propietarios 
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Propietarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Nombre.Contains(search) ||
                                         p.Apellido.Contains(search) ||
                                         p.Dni.Contains(search) ||
                                         p.Email.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var propietario = await _context.Propietarios.FirstOrDefaultAsync(m => m.IdPropietario == id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken] //Previene ataques csrf
        public async Task<IActionResult> Create([Bind("IdPropietario,Dni,Nombre,Apellido,Telefono,Email,Estado")] Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                bool existeDni = await _context.Propietarios.AnyAsync(p => p.Dni == propietario.Dni);
                if (existeDni)
                {
                    ModelState.AddModelError("Dni", "Ya existe un propietario registrado con este DNI.");
                    return View(propietario);
                }

                _context.Add(propietario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Propietario registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var propietario = await _context.Propietarios.FindAsync(id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPropietario,Dni,Nombre,Apellido,Telefono,Email,Estado")] Propietario propietario)
        {
            if (id != propietario.IdPropietario) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeDni = await _context.Propietarios.AnyAsync(p => p.Dni == propietario.Dni && p.IdPropietario != id);
                if (existeDni)
                {
                    ModelState.AddModelError("Dni", "El DNI ingresado ya pertenece a otro propietario.");
                    return View(propietario);
                }

                try
                {
                    _context.Update(propietario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Propietario actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Propietarios.Any(e => e.IdPropietario == propietario.IdPropietario))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var propietario = await _context.Propietarios.FirstOrDefaultAsync(m => m.IdPropietario == id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propietario = await _context.Propietarios.FindAsync(id);
            if (propietario != null)
            {
                _context.Propietarios.Remove(propietario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Propietario eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}