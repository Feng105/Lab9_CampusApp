using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class Category
    {
        [Key]
        public int ID { get; set; }
        //By convention,
        //a property named Id or <type name>Id
        //will be configured as the primary
        //key of an entity.

        [DisplayName("Category Name")]

        [Required(ErrorMessage = "Category name cannot be empty")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z\-\s]*$", ErrorMessage = "Name must start with letters")]
        public string Name { get; set; }
        public virtual ICollection<Product> Products{get; set;}
    }
}