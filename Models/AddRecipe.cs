using project.Models;

public class AddRecipeModel
{
    public string RecipeName { get; set; }
    public string Ingredients { get; set; }
    public string Instruction { get; set; }
    public int CategoryID { get; set; }

    public IFormFile? Photo { get; set; }

    public List<Category> Categories { get; set; } = new List<Category>();
}