using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioReserva
    {
        IList<Reserva> ObtenerTodos();
        Reserva? ObtenerPorId(int id);
        bool HaySolapamiento(int idInmueble, DateTime inicio, DateTime fin, int? idReservaActual = null);
        int Alta(Reserva reserva);
        int Modificacion(Reserva reserva);
        int Baja(int id);
        int FinalizarConMulta(int idReserva, DateTime fechaTerminacion, decimal multa, int usuarioId);
    }
}
