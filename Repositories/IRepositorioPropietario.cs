using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioPropietario
    {
        IList<Propietario> ObtenerTodos(string? search = null, int page = 1, int pageSize = 10);
        int Contar(string? search = null);
        Propietario? ObtenerPorId(int id);
        int Alta(Propietario p);
        int Modificacion(Propietario p);
        int Baja(int id);
    }
}
