using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaULP.Models
{
    public class Reserva
    {
        [Key]
        [Display(Name = "N° Reserva")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Desde")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Hasta")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(1);

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Fin Original")]
        public DateTime FechaFinOriginal { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Terminacion Anticipada")]
        public DateTime? FechaTerminacion { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto Diario")]
        public decimal MontoDiario { get; set; }

        [Display(Name = "Multa")]
        public decimal Multa { get; set; } = 0.00m;

        [StringLength(30)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Vigente";

        [Required(ErrorMessage = "Debe seleccionar un inmueble.")]
        [Display(Name = "Inmueble")]
        public int IdInmueble { get; set; }

        [ForeignKey(nameof(IdInmueble))]
        public Inmueble? Inmueble { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un inquilino.")]
        [Display(Name = "Inquilino")]
        public int IdInquilino { get; set; }

        [ForeignKey(nameof(IdInquilino))]
        public Inquilino? Inquilino { get; set; }

        // Propiedad calculada
        [NotMapped]
        [Display(Name = "Total Dias")]
        public int CantidadDias => (FechaFin - FechaInicio).Days > 0 ? (FechaFin - FechaInicio).Days : 1;

        [NotMapped]
        [Display(Name = "Importe Total")]
        public decimal ImporteTotal => CantidadDias * MontoDiario;
    }
}
