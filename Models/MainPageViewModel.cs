using System.Collections.Generic;

namespace project.Models
{
    public class MainPageViewModel
    {
        public List<CategoryWithRecipes> CategoryCards { get; set; }
        public List<Recipe> Recipes { get; set; }
    }
}

// Это модель представления (ViewModel), то есть класс, который объединяет все данные, которые нужны твоему представлению (Razor-странице) для корректного отображения. 
// Тут объединяются эти разные данные в один объект, чтобы передать их из контроллера во View. Можно структурировано собрать все данные для View и передать единым объектом.