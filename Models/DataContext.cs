using Microsoft.EntityFrameworkCore;

namespace InmobiliariaULP.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<TipoInmueble> TiposInmueble { get; set; }
    }
}
