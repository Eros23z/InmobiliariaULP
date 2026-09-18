using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public abstract class Persona
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener 7 u 8 dígitos numéricos.")]
        [Display(Name = "DNI")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Apellido}, {Nombre}";

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