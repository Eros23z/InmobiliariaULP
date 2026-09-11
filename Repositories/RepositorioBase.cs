using Microsoft.Extensions.Configuration;

namespace InmobiliariaULP.Repositories
{
    public abstract class RepositorioBase
    {
        protected readonly string _connectionString;

        public RepositorioBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }
    }
}