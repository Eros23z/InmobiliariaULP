using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioTipoInmueble
    {
        IList<TipoInmueble> ObtenerTodos();
        TipoInmueble? ObtenerPorId(int id);
        int Alta(TipoInmueble tipo);
        int Modificacion(TipoInmueble tipo);
        int Baja(int id);
    }
}
