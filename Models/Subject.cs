using System.ComponentModel.DataAnnotations;

namespace final_project_Api.Models
{
    public class Subject
    {
        [Key]
        public int Subject_ID { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot be longer than 250 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Subject Name is required.")]
        [StringLength(100, ErrorMessage = "Subject Name cannot be longer than 100 characters.")]
        public string Subject_Name { get; set; }
        public  virtual ICollection<Teacher>? teachers { get; set; }
    }
}
