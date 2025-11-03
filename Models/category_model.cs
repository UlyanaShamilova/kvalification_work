using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    [Table("category")]
    public class Category
    {
        public int categoryID { get; set; }
        public string category_name { get; set; }
        public byte[]? Photo { get; set; }
    }
}