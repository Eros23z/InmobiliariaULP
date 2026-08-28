// Models/Persona.cs
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public abstract class Persona
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(20, ErrorMessage = "El DNI no puede superar los 20 caracteres.")]
        [Display(Name = "DNI")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono de contacto es obligatorio.")]
        [Phone(ErrorMessage = "Formato de telefono no válido.")]
        [Display(Name = "Telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electronico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no valido.")]
        [Display(Name = "Correo Electronico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        public override string ToString() => $"{Nombre} {Apellido} ({Dni})";
    }
}