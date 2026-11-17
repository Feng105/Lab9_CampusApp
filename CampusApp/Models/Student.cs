using System.ComponentModel.DataAnnotations;

namespace CampusApp.Models
{
    public class Student
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        [RegularExpression(@"^[A-Z][A-Za-z]*(\s[A-Z][A-Za-z]*)*$",
            ErrorMessage = "Name must start with a capital letter; letters and spaces only.")]
        [Display(Name = "Student Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9\s,\.\-/]*$",
            ErrorMessage = "Address: letters, numbers, spaces, commas, dots and hyphens only.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        public int? CampusID { get; set; }

        public virtual UniversityCampus Campus { get; set; }
    }
}
