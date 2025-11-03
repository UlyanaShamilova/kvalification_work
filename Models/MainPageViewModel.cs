using System.Collections.Generic;

namespace project.Models
{
    public class MainPageViewModel
    {
        public List<CategoryWithRecipes> CategoryCards { get; set; }
        public List<Recipe> Recipes { get; set; }
    }
}