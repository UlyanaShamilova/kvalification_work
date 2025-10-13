using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    [Table("category")]  // указываем точное имя таблицы в базе, поскольку EF Core по умолчанию пытается работать с таблицами во множественном числе.
    public class Category
    {
        public int categoryID { get; set; }
        public string category_name { get; set; }
        public byte[]? Photo { get; set; }
    }
}
