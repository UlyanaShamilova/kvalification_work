using project.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    [Table("saved_recipes")]
    public class SavedRecipe
    {
        [Key]
        [Column("savedID")]
        public int savedID { get; set; }

        [Column("userID")]
        public int userID { get; set; }

        [Column("recipeID")]
        public int recipeID { get; set; }

        public User User { get; set; }
        public Recipe Recipe { get; set; }
    }
}
