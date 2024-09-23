using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Student_Teacher_Feedback
    {
        [Key]
        public int Student_Teacher_Feedback_Id { get; set; }
        [ForeignKey("Teacher")]
        public string? Teacher_ID { get; set; }
        public virtual Teacher? Teacher { get; set; }
        [ForeignKey("Student")]
        public string? Student_ID { get; set; }  
        public virtual Student? Student { get; set; }
        public string Feedback { get; set; }
    }
}
