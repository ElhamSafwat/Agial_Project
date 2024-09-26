using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace final_project_Api.Models
{
    public class Student
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Column(TypeName = "date")]
        public DateTime enrollmentDate { get; set; }
        [RegularExpression("^(أبتدائي|أعدادي|ثانوي)$", ErrorMessage = "القيمه يجب ان تكون أبتدائي او اعدادي او ثانوي")]
        public string Stage { get; set; }
        public int Level {  get; set; }

        [ForeignKey("parent")]
        public string Parent_ID { get; set; }
        public virtual Parent parent { get; set; }
        public virtual ICollection<Student_Class>? Student_Classes { get; set; }

        // realation with Payment table
        public virtual ICollection<Payment>? Payments { get; set; }

        // realation with Student_Teacher_FeedBack table
        public virtual ICollection<Student_Teacher_Feedback>? Students_Teachers_FeedBack { get; set; }

        // realation with Session_Student table
        public virtual ICollection<Session_Student>? Session_Students { get; set; }

        // realation with Student_Exam table
        public virtual ICollection<Student_Exam>? Student_Exams { get; set; }

    }
}
