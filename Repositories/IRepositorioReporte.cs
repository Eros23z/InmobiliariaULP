using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioReporte
    {
        IList<Inmueble> ObtenerInmueblesPorDisponibilidad(bool? disponible);
        IList<Inmueble> ObtenerInmueblesPorPropietario(int idPropietario);
        IList<InmuebleConReservasVM> ObtenerMasReservadosUltimoAno();
        IList<Inmueble> ObtenerInmueblesSinReservasEnDias(int dias);
        IList<Reserva> ObtenerReservasVigentesEnRango(DateTime desde, DateTime hasta);
        IList<Reserva> ObtenerReservasPorFinalizar(int dias);
        IList<Inmueble> BuscarInmueblesDisponibles(DateTime fechaInicio, DateTime fechaFin);
    }
}