namespace InmobiliariaULP.Models
{
    public class InmuebleConReservasVM
    {
        public Inmueble Inmueble { get; set; } = new Inmueble();
        public int CantidadReservas { get; set; }
    }
}