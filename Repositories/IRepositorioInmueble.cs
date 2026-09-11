using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioInmueble
    {
        IList<Inmueble> ObtenerTodos(string? search = null, int? idTipo = null, bool? soloDisponibles = null);
        Inmueble? ObtenerPorId(int id);
        int Alta(Inmueble inmueble);
        int Modificacion(Inmueble inmueble);
        int Baja(int id);
    }
}
