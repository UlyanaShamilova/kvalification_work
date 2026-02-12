using System.ComponentModel.DataAnnotations;
using project.Models;

public class AddRecipeModel
{
    public int? RecipeID { get; set; } 
    
    [Required(ErrorMessage = "Введіть назву рецепту")]
    public string RecipeName { get; set; }
    
    [Required(ErrorMessage = "Вкажіть інгредієнти")]
    public string Ingredients { get; set; }
    
    [Required(ErrorMessage = "Опишіть інструкцію приготування")]
    public string Instruction { get; set; }

    [Required(ErrorMessage = "Оберіть категорію")]
    public int CategoryID { get; set; }

    public IFormFile? Photo { get; set; }

    public List<Category> Categories { get; set; } = new List<Category>();

    public string? TimeCooking { get; set; }
}