using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioInquilino
    {
        IList<Inquilino> ObtenerTodos(string? search = null, int page = 1, int pageSize = 10);
        int Contar(string? search = null);
        Inquilino? ObtenerPorId(int id);
        int Alta(Inquilino inq);
        int Modificacion(Inquilino inq);
        int Baja(int id);
    }
}
