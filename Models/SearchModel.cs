using System.Collections.Generic;

namespace project.Models
{
    public class SearchModel
    {
        public string Query { get; set; }
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public List<Category> AllCategories { get; set; } = new List<Category>();
        public List<Recipe> FilteredRecipes { get; set; } = new List<Recipe>();
        public List<Recipe> AllRecipes { get; set; } = new List<Recipe>();
    }
}