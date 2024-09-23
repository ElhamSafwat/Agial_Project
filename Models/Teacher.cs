using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Teacher
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [Column(TypeName = "date")]
        public DateTime HireDate { get; set; }
        public bool IsDelete {  get; set; }
        [ForeignKey("subject")]
        public int Subject_ID {  get; set; }
        public virtual Subject subject { get; set; }

        //relation with Exam table
        public virtual ICollection<Exam>? exams { get; set; }

        //relation with Parent_Teacher table
        public virtual ICollection<Parent_Teacher_Feedback>? Parent_Teacher_FeedBacks { get; set; }

        //relation with Teacher_Stage table
        public virtual ICollection<Teacher_Stage>? teacher_Stages { get; set; }

        //relation with Teacher_Class table
        public virtual ICollection<Teacher_Class>? teacher_Classes { get; set; }

        //relation with Student_Teacher_FeedBack table
        public virtual ICollection<Student_Teacher_Feedback>? student_Teacher_FeedBacks { get; set; }
    }
}
