using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly DataContext _context;

        public InmueblesController(DataContext context)
        {
            _context = context;
        }

        // Carga de combos desplegables
        private async Task CargarListasDesplegables(int? idPropietario = null, int? idTipo = null)
        {
            var propietarios = await _context.Propietarios
                .Where(p => p.Estado)
                .OrderBy(p => p.Apellido)
                .ToListAsync();

            var tipos = await _context.TiposInmueble
                .OrderBy(t => t.Descripcion)
                .ToListAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "NombreCompleto", idPropietario);
            ViewBag.IdTipoInmueble = new SelectList(tipos, "IdTipoInmueble", "Descripcion", idTipo);
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index(string search, int? tipoId, bool? soloDisponibles, int page = 1, int pageSize = 10)
        {
            var query = _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Direccion.Contains(search) ||
                                         i.Propietario!.Apellido.Contains(search) ||
                                         i.Propietario!.Nombre.Contains(search));
            }

            if (tipoId.HasValue && tipoId.Value > 0)
            {
                query = query.Where(i => i.IdTipoInmueble == tipoId.Value);
            }

            if (soloDisponibles.HasValue && soloDisponibles.Value)
            {
                query = query.Where(i => i.Disponible);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(i => i.Direccion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TipoId = tipoId;
            ViewBag.SoloDisponibles = soloDisponibles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TiposFiltro = new SelectList(await _context.TiposInmueble.ToListAsync(), "IdTipoInmueble", "Descripcion", tipoId);

            return View(items);
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .FirstOrDefaultAsync(m => m.IdInmueble == id);

            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasDesplegables();
            return View();
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inmueble);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Inmueble publicado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();

            await CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble)
        {
            if (id != inmueble.IdInmueble) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Inmueble actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inmuebles.Any(e => e.IdInmueble == inmueble.IdInmueble))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .FirstOrDefaultAsync(m => m.IdInmueble == id);

            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool tieneReservas = await _context.Reservas.AnyAsync(r => r.IdInmueble == id);
            if (tieneReservas)
            {
                TempData["Error"] = "No se puede eliminar el inmueble porque registra reservas historicas o activas. Se recomienda suspender su disponibilidad.";
                return RedirectToAction(nameof(Index));
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Inmueble eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Endpoint para obtener el precio por dia via fetch/AJAX
        [HttpGet]
        public async Task<IActionResult> ObtenerPrecio(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();
            return Json(new { precio = inmueble.PrecioPorDia, porcentaje = inmueble.PorcentajeReserva });
        }
    }
}