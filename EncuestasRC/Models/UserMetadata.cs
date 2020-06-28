using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EncuestasRC.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {

    }

    public class UserMetadata
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de empleado es requerido.")]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string PasswordHash { get; set; }

        public string Role { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        public string Email { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }
}