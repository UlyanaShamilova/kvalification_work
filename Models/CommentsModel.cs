using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    [Table("rewiews")]
    public class Comments
    {
        [Key]
        public int rewiewID { get; set; }
        public string text { get; set; }
        public int userID { get; set; }
        public User User { get; set; }
        public int recipeID { get; set; }
        public Recipe Recipe { get; set; }
        public int? parentID { get; set; }
        public Comments Parent { get; set; }
        public ICollection<Comments> Replies { get; set; }
    }
}