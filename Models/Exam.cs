using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Exam
    {
        [Key]
        public int Exam_ID { get; set; }
        [Column(TypeName = "date")]
        public DateTime Exam_Date { get; set; }
        public float Start_Time { get; set; }
        public float End_Time { get; private set; }
        public int Min_Degree { get; set; }
        public int Max_Degree { get; set; }
        [ForeignKey("Tech")]
        public string? Teacher_ID { get; set; }
        public virtual Teacher? Tech { get; set; }
        public virtual ICollection<Student_Exam>? Student_Exam { get; set; }
    }
}
