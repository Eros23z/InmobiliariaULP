using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaULP.Models
{
    public class Inmueble
    {
        [Key]
        [Display(Name = "Codigo")]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "La direccion es requerida.")]
        [StringLength(150)]
        [Display(Name = "Direccion")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cupo es obligatorio.")]
        [Range(1, 50, ErrorMessage = "El cupo debe ser al menos de 1 persona.")]
        [Display(Name = "Cupo (Personas)")]
        public int Cupo { get; set; }

        [Required]
        [Display(Name = "Latitud")]
        public decimal Latitud { get; set; }

        [Required]
        [Display(Name = "Longitud")]
        public decimal Longitud { get; set; }

        [Required(ErrorMessage = "El precio por dia es obligatorio.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio por Dia")]
        public decimal PrecioPorDia { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Display(Name = "% Seña/Reserva")]
        public decimal PorcentajeReserva { get; set; } = 100.00m;

        [Display(Name = "Disponible para Alquiler")]
        public bool Disponible { get; set; } = true;

        [Display(Name = "Imagen")]
        public string? ImagenPortada { get; set; }

        [Required(ErrorMessage = "Debe asignar un propietario.")]
        [Display(Name = "Propietario")]
        public int IdPropietario { get; set; }

        [ForeignKey(nameof(IdPropietario))]
        public Propietario? Propietario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de inmueble.")]
        [Display(Name = "Tipo")]
        public int IdTipoInmueble { get; set; }

        [ForeignKey(nameof(IdTipoInmueble))]
        public TipoInmueble? TipoInmueble { get; set; }

        public override string ToString() => $"{Direccion} - Cupo: {Cupo} ({TipoInmueble?.Descripcion})";
    }
}
