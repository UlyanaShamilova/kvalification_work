using project.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace project.Models
{
    [Table("user")]
    public class User
    {
        public int userID { get; set; }

        [Required(ErrorMessage = "Введіть ім'я")]
        public string username { get; set; }

        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Неправильний email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        public string password { get; set; }

        public ICollection<SavedRecipe> SavedRecipes { get; set; }

        public ICollection<Recipe> Recipes { get; set; }

    }
}
