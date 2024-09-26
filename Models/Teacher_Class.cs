using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Teacher_Class
    {
        [Key]
        public int TC_ID { get; set; }
        [ForeignKey("Teacher")]
        public string? Teacher_ID { get; set; }
        public int? Subject_ID { get; set; }
        public virtual Teacher? Teacher { get; set; }
        [ForeignKey("Class")]
        public int? Class_ID { get; set; }
        public virtual Subject? Subjects { get; set; }
        public virtual Class? Class { get; set; }
        public virtual ICollection<Session>? Sessions { get; set; }

    }
}
