using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;

namespace InmobiliariaULP.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repoReserva;
        private readonly IRepositorioInmueble _repoInmueble;
        private readonly IRepositorioInquilino _repoInquilino;
        private readonly IRepositorioPago _repoPago;

        public ReservasController(
            IRepositorioReserva repoReserva,
            IRepositorioInmueble repoInmueble,
            IRepositorioInquilino repoInquilino,
            IRepositorioPago repoPago)
        {
            _repoReserva = repoReserva;
            _repoInmueble = repoInmueble;
            _repoInquilino = repoInquilino;
            _repoPago = repoPago;
        }

        private void CargarListasDesplegables(int? idInmueble = null, int? idInquilino = null)
        {
            var inmuebles = _repoInmueble.ObtenerTodos(soloDisponibles: true);
            var inquilinos = _repoInquilino.ObtenerTodos(null, 1, 500)
                .Where(i => i.Estado)
                .OrderBy(i => i.Apellido);

            ViewBag.IdInmueble = new SelectList(inmuebles, "IdInmueble", "DescripcionCompleta", idInmueble);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", idInquilino);
        }

        // GET: Reservas
        public IActionResult Index(string search, string estado, DateTime? fechaDesde, DateTime? fechaHasta, int page = 1, int pageSize = 10)
        {
            var lista = _repoReserva.ObtenerTodos();

            if (!string.IsNullOrWhiteSpace(search))
            {
                lista = lista.Where(r => (r.Inquilino != null && (r.Inquilino.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) || r.Inquilino.Apellido.Contains(search, StringComparison.OrdinalIgnoreCase))) ||
                                         (r.Inmueble != null && r.Inmueble.Direccion.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                lista = lista.Where(r => r.Estado == estado).ToList();
            }

            if (fechaDesde.HasValue)
            {
                lista = lista.Where(r => r.FechaInicio >= fechaDesde.Value).ToList();
            }

            if (fechaHasta.HasValue)
            {
                lista = lista.Where(r => r.FechaFin <= fechaHasta.Value).ToList();
            }

            int totalItems = lista.Count;
            var paginados = lista
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Estado = estado;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(paginados);
        }

        // GET: Reservas/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var reserva = _repoReserva.ObtenerPorId(id.Value);
            if (reserva == null) return NotFound();

            // Cargar pagos asociados mediante su repositorio
            reserva.Pagos = _repoPago.ObtenerPorReserva(reserva.IdReserva);

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View(new Reserva
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(1)
            });
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdReserva,FechaInicio,FechaFin,MontoDiario,IdInmueble,IdInquilino")] Reserva reserva)
        {
            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser estrictamente posterior a la de inicio.");
            }

            var inmueble = _repoInmueble.ObtenerPorId(reserva.IdInmueble);
            if (inmueble != null && reserva.MontoDiario <= 0)
            {
                reserva.MontoDiario = inmueble.PrecioPorDia;
            }

            bool superpuesta = _repoReserva.HaySolapamiento(reserva.IdInmueble, reserva.FechaInicio, reserva.FechaFin);
            if (superpuesta)
            {
                ModelState.AddModelError(string.Empty, "El inmueble ya se encuentra reservado en el rango de fechas seleccionado.");
            }

            if (ModelState.IsValid)
            {
                reserva.FechaFinOriginal = reserva.FechaFin;
                reserva.Estado = "Vigente";

                _repoReserva.Alta(reserva);
                TempData["Success"] = "Reserva generada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var reserva = _repoReserva.ObtenerPorId(id.Value);
            if (reserva == null) return NotFound();

            CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdReserva,FechaInicio,FechaFin,FechaFinOriginal,FechaTerminacion,MontoDiario,Multa,Estado,IdInmueble,IdInquilino")] Reserva reserva)
        {
            if (id != reserva.IdReserva) return NotFound();

            if (reserva.FechaFin <= reserva.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser estrictamente posterior a la de inicio.");
            }

            bool superpuesta = _repoReserva.HaySolapamiento(reserva.IdInmueble, reserva.FechaInicio, reserva.FechaFin, reserva.IdReserva);
            if (superpuesta)
            {
                ModelState.AddModelError(string.Empty, "El rango de fechas colisiona con otra reserva activa del mismo inmueble.");
            }

            if (ModelState.IsValid)
            {
                _repoReserva.Modificacion(reserva);
                TempData["Success"] = "Reserva actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(reserva.IdInmueble, reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var reserva = _repoReserva.ObtenerPorId(id.Value);
            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repoReserva.Baja(id);
            TempData["Success"] = "Reserva cancelada y eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerUsuarioActualId()
        {
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimId, out int idUsuario))
            {
                return idUsuario;
            }
            return 1;
        }

        public IActionResult Finalizar(int? id)
        {
            if (id == null) return NotFound();

            var reserva = _repoReserva.ObtenerPorId(id.Value);
            if (reserva == null) return NotFound();

            if (reserva.Estado != "Vigente")
            {
                TempData["Error"] = "Solo se pueden rescindir reservas que se encuentren en estado 'Vigente'.";
                return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
            }

            ViewBag.FechaFinPactada = reserva.FechaFinOriginal ?? reserva.FechaFin;
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(int id, DateTime fechaTerminacion)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();

            var fechaFinPactada = reserva.FechaFinOriginal ?? reserva.FechaFin;

            // Validaciones sobre la fecha de terminación
            if (fechaTerminacion.Date < reserva.FechaInicio.Date)
            {
                ModelState.AddModelError("fechaTerminacion", "La fecha de corte no puede ser anterior a la fecha de inicio.");
            }
            if (fechaTerminacion.Date >= fechaFinPactada.Date)
            {
                ModelState.AddModelError("fechaTerminacion", "Para terminar anticipadamente, la fecha debe ser anterior a la fecha de fin pactada.");
            }

            if (ModelState.IsValid)
            {
                // Dias totales pactados originalmente
                int diasTotales = (fechaFinPactada.Date - reserva.FechaInicio.Date).Days;
                if (diasTotales <= 0) diasTotales = 1;

                // Dias efectivamente cumplidos y dias restantes no utilizados
                int diasCumplidos = (fechaTerminacion.Date - reserva.FechaInicio.Date).Days;
                int diasRestantes = (fechaFinPactada.Date - fechaTerminacion.Date).Days;

                decimal costoTotalRestante = diasRestantes * reserva.MontoDiario;
                decimal multa = 0;

                // si se cumplie menos de la mitad del tiempo entonces queda 50% del saldo restante sino el 25%
                if (diasCumplidos < (diasTotales / 2.0))
                {
                    multa = Math.Round(costoTotalRestante * 0.50m, 2);
                }
                else
                {
                    multa = Math.Round(costoTotalRestante * 0.25m, 2);
                }

                try
                {
                    int usuarioId = ObtenerUsuarioActualId();
                    _repoReserva.FinalizarConMulta(reserva.IdReserva, fechaTerminacion, multa, usuarioId);

                    TempData["Success"] = $"Reserva finalizada exitosamente. Se aplicó y registró una multa de ${multa:N2}.";
                    return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error al procesar la finalización: " + ex.Message);
                }
            }

            ViewBag.FechaFinPactada = fechaFinPactada;
            return View(reserva);
        }

        public IActionResult Renovar(int? id)
        {
            if (id == null) return NotFound();

            var reservaAnterior = _repoReserva.ObtenerPorId(id.Value);
            if (reservaAnterior == null) return NotFound();

            var nuevaReserva = new Reserva
            {
                IdInmueble = reservaAnterior.IdInmueble,
                IdInquilino = reservaAnterior.IdInquilino,
                MontoDiario = reservaAnterior.MontoDiario,
                FechaInicio = reservaAnterior.FechaFin,
                FechaFin = reservaAnterior.FechaFin.AddDays(7),
                Inmueble = reservaAnterior.Inmueble,
                Inquilino = reservaAnterior.Inquilino
            };

            CargarListasDesplegables(nuevaReserva.IdInmueble, nuevaReserva.IdInquilino);
            ViewBag.EsRenovacion = true;
            ViewBag.IdReservaOrigen = reservaAnterior.IdReserva;

            return View("Create", nuevaReserva);
        }
    }
}