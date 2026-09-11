using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;

namespace InmobiliariaULP.Controllers
{
    public class PagosController : Controller
    {
        private readonly IRepositorioPago _repoPago;
        private readonly IRepositorioReserva _repoReserva;

        public PagosController(IRepositorioPago repoPago, IRepositorioReserva repoReserva)
        {
            _repoPago = repoPago;
            _repoReserva = repoReserva;
        }

        private void CargarComboReservas(int? idReserva = null)
        {
            var reservas = _repoReserva.ObtenerTodos()
                .Select(r => new
                {
                    r.IdReserva,
                    Descripcion = $"Reserva #{r.IdReserva} - {r.Inquilino?.Apellido} ({r.Inmueble?.Direccion})"
                });

            ViewBag.IdReserva = new SelectList(reservas, "IdReserva", "Descripcion", idReserva);
        }

        // GET: Pagos
        public IActionResult Index(int? idReserva, string search, int page = 1, int pageSize = 10)
        {
            var items = _repoPago.ObtenerTodos(idReserva, search);

            if (idReserva.HasValue && idReserva.Value > 0)
            {
                ViewBag.ReservaFiltro = _repoReserva.ObtenerPorId(idReserva.Value);
            }

            int totalItems = items.Count;
            var paginados = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.IdReserva = idReserva;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(paginados);
        }

        // GET: Pagos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = _repoPago.ObtenerPorId(id.Value);
            if (pago == null) return NotFound();

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create(int? idReserva)
        {
            CargarComboReservas(idReserva);
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
        public IActionResult Create([Bind("IdPago,Concepto,FechaPago,Importe,IdReserva")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                pago.Anulado = false;
                _repoPago.Alta(pago);

                TempData["Success"] = "Pago registrado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }

            CargarComboReservas(pago.IdReserva);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var pago = _repoPago.ObtenerPorId(id.Value);
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
        public IActionResult Edit(int id, string concepto)
        {
            var pagoDb = _repoPago.ObtenerPorId(id);
            if (pagoDb == null) return NotFound();

            if (pagoDb.Anulado)
            {
                TempData["Error"] = "No se puede editar un pago anulado.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(concepto))
            {
                ModelState.AddModelError("Concepto", "El concepto no puede quedar vacío.");
                return View(pagoDb);
            }

            _repoPago.ModificarConcepto(id, concepto);
            TempData["Success"] = "Concepto de pago actualizado con éxito.";
            return RedirectToAction(nameof(Index), new { idReserva = pagoDb.IdReserva });
        }

        // GET: Pagos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var pago = _repoPago.ObtenerPorId(id.Value);
            if (pago == null) return NotFound();

            return View(pago);
        }

        // POST: Pagos/Delete/5 (Baja lógica)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var pago = _repoPago.ObtenerPorId(id);
            if (pago != null)
            {
                _repoPago.Anular(id);
                TempData["Success"] = "El pago ha sido anulado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}