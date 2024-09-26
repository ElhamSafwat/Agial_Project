
using System.ComponentModel.DataAnnotations;

namespace final_project_Api.Models
{
    public class Class
    {
        [Key]
        public int Class_ID { get; set; }
        [Required]
        public string Stage { get; set; }
        public int Level { get; set; }
        [Required]
        public string Class_Name { get; set; }
        public virtual  ICollection<Student_Class>? Student_Class { get; set; }
        public virtual ICollection<Teacher_Class>? Teacher_Class { get; set; }
    }
}
