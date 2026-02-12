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
        public string? Photo { get; set; }  
        public TimeSpan? time_cooking { get; set; }
        public string ingredients { get; set; }
        public string? instruction { get; set; }

        public int authorUserID { get; set; }
        
        [ForeignKey(nameof(authorUserID))]
        public User Author { get; set; }

        [NotMapped]
        public string[] IngredientsArr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ingredients)) return Array.Empty<string>();

                string[] parts = ingredients.Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Trim();
                }

                return parts;
            }
        }

        [NotMapped]
        public string[] InstructionsArr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(instruction)) return Array.Empty<string>();

                string[] parts = instruction.Split('.', StringSplitOptions.RemoveEmptyEntries);

                int count = 0;
                for (int i = 0; i < parts.Length; i++)
                {
                    string step = parts[i].Trim();

                    if (step.Length == 1 && char.IsDigit(step[0])) continue;

                    if (!string.IsNullOrEmpty(step)) count++;
                }

                string[] result = new string[count];
                int index = 0;

                for (int i = 0; i < parts.Length; i++)
                {
                    string step = parts[i].Trim();

                    if (step.Length == 1 && char.IsDigit(step[0])) continue;

                    if (!string.IsNullOrEmpty(step))
                    {
                        result[index] = step;
                        index++;
                    }
                }

                return result;
            }
        }

        public Category? Category { get; set; }

        [NotMapped]
        public bool IsSaved { get; set; }

        public ICollection<SavedRecipe> SavedByUsers { get; set; }
        public ICollection<Comments> Comments { get; set; }
    }
}
