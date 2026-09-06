using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaULP.Models
{
    public class Pago
    {
        [Key]
        [Display(Name = "N° Pago")]
        public int IdPago { get; set; }

        [Required(ErrorMessage = "El concepto del pago es obligatorio.")]
        [StringLength(150, ErrorMessage = "El concepto no puede exceder 150 caracteres.")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El importe es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El importe debe ser mayor a 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Display(Name = "Anulado")]
        public bool Anulado { get; set; } = false;

        [Required(ErrorMessage = "Debe vincular el pago a una reserva.")]
        [Display(Name = "Reserva")]
        public int IdReserva { get; set; }

        [ForeignKey(nameof(IdReserva))]
        public Reserva? Reserva { get; set; }
    }
}