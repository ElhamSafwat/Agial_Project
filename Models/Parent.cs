using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Parent
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Parent_Teacher_Feedback>? Parent_Teacher_feadback { get; set; }
        public virtual ICollection<Student>? Students { get; set; }

    }
}
