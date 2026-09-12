using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioPago
    {
        IList<Pago> ObtenerTodos(int? idReserva = null, string? search = null);
        IList<Pago> ObtenerPorReserva(int idReserva);
        Pago? ObtenerPorId(int id);
        int Alta(Pago pago);
        int ModificarConcepto(int id, string concepto);
        int Anular(int id, int usuarioAnulaId);
    }
}
