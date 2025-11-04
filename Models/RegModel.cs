using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class RegModel
    {
        [Display(Name = "Введіть ім'я: ")]
        [Required(ErrorMessage = "Введіть ім'я")]
        public string username { get; set; }

        [Display(Name = "Введіть email: ")]
        [EmailAddress(ErrorMessage = "Неправильний email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email має містити @gmail.com")]
        [Required(ErrorMessage = "Введіть email")]
        public string email { get; set; }

        [Display(Name = "Введіть пароль: ")]
        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        [Required(ErrorMessage = "Введіть пароль")]
        public string password { get; set; }
    }
}
