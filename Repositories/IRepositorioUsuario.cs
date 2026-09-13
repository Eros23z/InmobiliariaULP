using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public interface IRepositorioUsuario
    {
        IList<Usuario> ObtenerTodos();
        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorEmail(string email);
        int Alta(Usuario u);
        int Modificacion(Usuario u);
        int ModificarPerfil(Usuario u);
        int ModificarClave(int id, string nuevaClave);
        int Baja(int id);
    }
}