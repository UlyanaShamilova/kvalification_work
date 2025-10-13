using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Recipe
    {
        [Key]
        [Column("recipeID")]
        public int recipeID { get; set; }
        public string? recipe_name { get; set; }
        public int categoryID { get; set; }
        public string Photo { get; set; }  
        public TimeSpan? time_cooking { get; set; }
        public string ingredients { get; set; }
        public string? instruction { get; set; }

        [NotMapped] // чтобы EF не пытался сохранить это поле в БД
        public string[] IngredientsArr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ingredients)) return Array.Empty<string>();

                // разделяем строку на массив
                string[] parts = ingredients.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // убираем пробелы вручную
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Trim();
                }

                return parts;
            }
        }

        [NotMapped] // чтобы EF не пытался сохранить это поле в БД
        public string[] InstructionsArr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(instruction)) return Array.Empty<string>();

                // Разделяем по точке
                string[] parts = instruction.Split('.', StringSplitOptions.RemoveEmptyEntries);

                // Считаем количество непустых шагов (без одиночных цифр)
                int count = 0;
                for (int i = 0; i < parts.Length; i++)
                {
                    string step = parts[i].Trim();

                    // Пропускаем шаг, если это только цифра (например "1", "2", "3")
                    if (step.Length == 1 && char.IsDigit(step[0])) continue;

                    if (!string.IsNullOrEmpty(step)) count++;
                }

                // Создаём массив правильного размера
                string[] result = new string[count];
                int index = 0;

                for (int i = 0; i < parts.Length; i++)
                {
                    string step = parts[i].Trim();

                    if (step.Length == 1 && char.IsDigit(step[0])) continue; // пропускаем числа

                    if (!string.IsNullOrEmpty(step))
                    {
                        result[index] = step;
                        index++;
                    }
                }

                return result;
            }
        }

        public Category? Category { get; set; } // Знак ? после типа означает, что это nullable-ссылка — то есть свойство может быть равно null.

        [NotMapped]
        public bool IsSaved { get; set; } = false;

        public ICollection<SavedRecipe> SavedByUsers { get; set; }
        public ICollection<Comments> Comments { get; set; } // связь один ко многим
    }
}
