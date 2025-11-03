namespace project.Models
{
    public class CategoryWithRecipes
    {
        public Category Category { get; set; }
        public List<Recipe> Recipes { get; set; }
    }
}