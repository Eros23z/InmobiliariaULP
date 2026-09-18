using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioImagenInmueble
    {
        IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble);
        ImagenInmueble? ObtenerPorId(int idImagen);
        int Alta(ImagenInmueble imagen);
        int Borrar(int idImagen);
    }
}