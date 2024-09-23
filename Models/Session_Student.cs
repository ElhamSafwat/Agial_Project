using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Session_Student
    {
        [Key]
        public int Session_Student_Id { get; set; }
        [ForeignKey("Session")]
        public int? Session_ID { get; set; }
        public virtual Session? Session { get; set; }
        [ForeignKey("Student")]
        public string? Student_ID { get; set; }
        public virtual Student? Student { get; set; }
        public string? Assignment { get; set; }
        public bool Attendance { get; set; }
        public float Degree { get; set; }

    }
}
