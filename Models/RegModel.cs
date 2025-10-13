using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class RegModel
    {
        [Required(ErrorMessage = "Введіть ім'я")]
        public string username { get; set; }

        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Неправильний email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        public string password { get; set; }
    }
}
