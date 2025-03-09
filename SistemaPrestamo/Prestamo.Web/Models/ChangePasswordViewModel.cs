using System.ComponentModel.DataAnnotations;

namespace Prestamo.Web.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación de la contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Código de verificación")]
        public string? VerificationCode { get; set; } = null!;
    }
}
