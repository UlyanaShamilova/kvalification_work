using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class LogModel
    {
        [Display(Name = "Введіть ім'я: ")]
        [Required(ErrorMessage = "Введіть ім'я")]
        public string username { get; set; }

        [Display(Name = "Введіть пароль: ")]
        [Required(ErrorMessage = "Введіть пароль")]
        public string password { get; set; }
    }
}
