using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Student_Exam
    {
        [Key]
        public int Student_Exam_Id { get; set; }
        [ForeignKey("Student")]
        public string? Student_ID { get; set; }
        public virtual Student? Student { get; set; }
        [ForeignKey("Exam")]
        public int? Exam_ID { get; set; }
        public virtual Exam? Exam { get; set; }
        public float Degree { get; set; }

    }
}
