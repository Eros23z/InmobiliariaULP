using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InmobiliariaULP.Repositories;

namespace InmobiliariaULP.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IRepositorioReporte _repoReporte;
        private readonly IRepositorioPropietario _repoPropietario;

        public ReportesController(IRepositorioReporte repoReporte, IRepositorioPropietario repoPropietario)
        {
            _repoReporte = repoReporte;
            _repoPropietario = repoPropietario;
        }

        public IActionResult Index() => View();

        public IActionResult InmueblesPorDisponibilidad(bool? disponible)
        {
            ViewBag.Disponible = disponible;
            var resultado = _repoReporte.ObtenerInmueblesPorDisponibilidad(disponible);
            return View(resultado);
        }

        public IActionResult InmueblesPorPropietario(int? idPropietario)
        {
            var propietarios = _repoPropietario.ObtenerTodos();
            ViewBag.Propietarios = new SelectList(propietarios, "IdPropietario", "NombreCompleto", idPropietario);

            var resultado = idPropietario.HasValue
                ? _repoReporte.ObtenerInmueblesPorPropietario(idPropietario.Value)
                : new List<Models.Inmueble>();

            return View(resultado);
        }

        public IActionResult MasReservados()
        {
            var resultado = _repoReporte.ObtenerMasReservadosUltimoAno();
            return View(resultado);
        }

        public IActionResult SinReservas(int dias = 30)
        {
            ViewBag.Dias = dias;
            var resultado = _repoReporte.ObtenerInmueblesSinReservasEnDias(dias);
            return View(resultado);
        }

        public IActionResult ReservasVigentes(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = desde ?? DateTime.Today;
            var fechaHasta = hasta ?? DateTime.Today.AddDays(30);

            ViewBag.Desde = fechaDesde.ToString("yyyy-MM-dd");
            ViewBag.Hasta = fechaHasta.ToString("yyyy-MM-dd");

            var resultado = _repoReporte.ObtenerReservasVigentesEnRango(fechaDesde, fechaHasta);
            return View(resultado);
        }

        public IActionResult ReservasPorFinalizar(int dias = 7)
        {
            ViewBag.Dias = dias;
            var resultado = _repoReporte.ObtenerReservasPorFinalizar(dias);
            return View(resultado);
        }

        public IActionResult Disponibilidad(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                if (fechaFin <= fechaInicio)
                {
                    ModelState.AddModelError("", "La fecha de egreso debe ser posterior a la de ingreso.");
                    return View(new List<Models.Inmueble>());
                }

                ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
                ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

                var resultado = _repoReporte.BuscarInmueblesDisponibles(fechaInicio.Value, fechaFin.Value);
                return View(resultado);
            }

            ViewBag.FechaInicio = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
            return View(new List<Models.Inmueble>());
        }
    }
}