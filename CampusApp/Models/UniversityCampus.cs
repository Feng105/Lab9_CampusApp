using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusApp.Models
{
    public class UniversityCampus
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Campus name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        [RegularExpression(@"^[A-Z][A-Za-z]*(\s[A-Z][A-Za-z]*)*$",
            ErrorMessage = "Name must start with a capital letter; letters and spaces only.")]
        [Display(Name = "Campus Name")]
        public string Name { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
