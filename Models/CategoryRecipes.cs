namespace project.Models
{
    public class CategoryWithRecipes
    {
        public Category Category { get; set; }
        public List<Recipe> Recipes { get; set; }
    }
}

// Это вспомогательная модель, которая связывает одну категорию и список рецептов, относящихся к ней. Она нужна, чтобы удобно передавать и показывать, 
// например, карточку категории и под ней — примеры рецептов. Контроллер сразу формирует нужную структуру, а View просто отображает.