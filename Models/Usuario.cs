using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public class Usuario
    {
        [Key]
        [Display(Name = "N°")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Clave { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Empleado"; 

        public string? Avatar { get; set; }

        public bool Estado { get; set; } = true;

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Apellido}, {Nombre}";
    }
}