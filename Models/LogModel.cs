using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class LogModel
    {
        [Required(ErrorMessage = "Введіть ім'я")]
        public string username { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        public string password { get; set; }
    }
}
