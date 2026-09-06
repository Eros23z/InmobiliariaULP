using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Controllers
{
    public class PagosController : Controller
    {
        private readonly DataContext _context;

        public PagosController(DataContext context)
        {
            _context = context;
        }

        private async Task CargarComboReservas(int? idReserva = null)
        {
            var reservas = await _context.Reservas
                .Include(r => r.Inquilino)
                .Include(r => r.Inmueble)
                .Select(r => new
                {
                    r.IdReserva,
                    Descripcion = $"Reserva #{r.IdReserva} - {r.Inquilino!.Apellido} ({r.Inmueble!.Direccion})"
                })
                .ToListAsync();

            ViewBag.IdReserva = new SelectList(reservas, "IdReserva", "Descripcion", idReserva);
        }

        // GET: Pagos
        public async Task<IActionResult> Index(int? idReserva, string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inquilino)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inmueble)
                .AsQueryable();

            if (idReserva.HasValue && idReserva.Value > 0)
            {
                query = query.Where(p => p.IdReserva == idReserva.Value);
                ViewBag.ReservaFiltro = await _context.Reservas
                    .Include(r => r.Inquilino)
                    .Include(r => r.Inmueble)
                    .FirstOrDefaultAsync(r => r.IdReserva == idReserva.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Concepto.Contains(search) || 
                                         p.Reserva!.Inquilino!.Apellido.Contains(search) ||
                                         p.Reserva!.Inmueble!.Direccion.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.FechaPago)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.IdReserva = idReserva;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inquilino)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inmueble)
                .FirstOrDefaultAsync(m => m.IdPago == id);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // GET: Pagos/Create
        public async Task<IActionResult> Create(int? idReserva)
        {
            await CargarComboReservas(idReserva);
            var pago = new Pago
            {
                FechaPago = DateTime.Today
            };

            if (idReserva.HasValue)
            {
                pago.IdReserva = idReserva.Value;
            }

            return View(pago);
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPago,Concepto,FechaPago,Importe,IdReserva")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                pago.Anulado = false;
                _context.Add(pago);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Pago registrado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }

            await CargarComboReservas(pago.IdReserva);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inquilino)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inmueble)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null) return NotFound();

            if (pago.Anulado)
            {
                TempData["Error"] = "No se puede editar un pago anulado.";
                return RedirectToAction(nameof(Index));
            }

            return View(pago);
        }

        // POST: Pagos/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string concepto)
        {
            var pagoDb = await _context.Pagos.FindAsync(id);
            if (pagoDb == null) return NotFound();

            if (pagoDb.Anulado)
            {
                TempData["Error"] = "No se puede editar un pago anulado.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(concepto))
            {
                ModelState.AddModelError("Concepto", "El concepto no puede quedar vacío.");
                var pagoCompleto = await _context.Pagos
                    .Include(p => p.Reserva).ThenInclude(r => r!.Inquilino)
                    .Include(p => p.Reserva).ThenInclude(r => r!.Inmueble)
                    .FirstOrDefaultAsync(p => p.IdPago == id);
                return View(pagoCompleto);
            }

            // Regla de negocio: solo se edita el concepto
            pagoDb.Concepto = concepto;
            _context.Update(pagoDb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Concepto de pago actualizado con éxito.";
            return RedirectToAction(nameof(Index), new { idReserva = pagoDb.IdReserva });
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inquilino)
                .Include(p => p.Reserva)
                    .ThenInclude(r => r!.Inmueble)
                .FirstOrDefaultAsync(m => m.IdPago == id);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // POST: Pagos/Delete/5 (Baja lógica: anula el pago)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                // Regla de negocio: la eliminación es un cambio de estado
                pago.Anulado = true;
                _context.Update(pago);
                await _context.SaveChangesAsync();
                TempData["Success"] = "El pago ha sido anulado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}