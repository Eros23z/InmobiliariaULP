using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class ReservasController : Controller
    {
        private readonly DataContext _context;

        public ReservasController(DataContext context)
        {
            _context = context;
        }

        private async Task CargarListasDesplegables(int? idInmueble = null, int? idInquilino = null)
        {
            var inmuebles = await _context.Inmuebles
                .Where(i => i.Disponible)
                .Include(i => i.TipoInmueble)
                .ToListAsync();

            var inquilinos = await _context.Inquilinos
                .Where(i => i.Estado)
                .OrderBy(i => i.Apellido)
                .ToListAsync();

            ViewBag.IdInmueble = new SelectList(inmuebles, "IdInmueble", "DescripcionCompleta", idInmueble);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", idInquilino);
        }

        // GET: Reservas
        public async Task<IActionResult> Index(string search, string estado, DateTime? fechaDesde, DateTime? fechaHasta, int page = 1, int pageSize = 10)
        {
            var query = _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Inquilino)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Inquilino!.Nombre.Contains(search) ||
                                         r.Inquilino!.Apellido.Contains(search) ||
                                         r.Inmueble!.Direccion.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(r => r.FechaInicio >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(r => r.FechaFin <= fechaHasta.Value);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.FechaInicio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Estado = estado;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                    .ThenInclude(i => i!.Propietario)
                .Include(r => r.Inmueble)
                    .ThenInclude(i => i!.TipoInmueble)
                .Include(r => r.Inquilino)
                .FirstOrDefaultAsync(m => m.IdReserva == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // GET: Reservas/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasDesplegables();
            return View(new Reserva
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(1)
            });
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReserva,FechaInicio,FechaFin,MontoDiario,IdInmueble,IdInquilino")] Reserva reserva)
        {
            // Validacion sobre fechas
            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser estrictamente posterior a la de inicio.");
            }

            // Asignar el monto diario vigente del inmueble si no vino provisto
            var inmueble = await _context.Inmuebles.FindAsync(reserva.IdInmueble);
            if (inmueble != null && reserva.MontoDiario <= 0)
            {
                reserva.MontoDiario = inmueble.PrecioPorDia;
            }

            // Validacion de solapamiento de fechas con reservas vigentes
            bool superpuesta = await _context.Reservas.AnyAsync(r =>
                r.IdInmueble == reserva.IdInmueble &&
                r.Estado == "Vigente" &&
                reserva.FechaInicio < r.FechaFin &&
                reserva.FechaFin > r.FechaInicio
            );

            if (superpuesta)
            {
                ModelState.AddModelError(string.Empty, "El inmueble ya se encuentra reservado en el rango de fechas seleccionado.");
            }

            if (ModelState.IsValid)
            {
                reserva.FechaFinOriginal = reserva.FechaFin;
                reserva.Estado = "Vigente";

                _context.Add(reserva);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reserva generada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            await CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,FechaInicio,FechaFin,FechaFinOriginal,FechaTerminacion,MontoDiario,Multa,Estado,IdInmueble,IdInquilino")] Reserva reserva)
        {
            if (id != reserva.IdReserva) return NotFound();

            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser estrictamente posterior a la de inicio.");
            }

            bool superpuesta = await _context.Reservas.AnyAsync(r =>
                r.IdInmueble == reserva.IdInmueble &&
                r.IdReserva != reserva.IdReserva &&
                r.Estado == "Vigente" &&
                reserva.FechaInicio < r.FechaFin &&
                reserva.FechaFin > r.FechaInicio
            );

            if (superpuesta)
            {
                ModelState.AddModelError(string.Empty, "El rango de fechas colisiona con otra reserva activa del mismo inmueble.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Reserva actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reservas.Any(e => e.IdReserva == reserva.IdReserva))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Inquilino)
                .FirstOrDefaultAsync(m => m.IdReserva == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reserva cancelada y eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}