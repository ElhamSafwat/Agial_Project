using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.DTO
{
    public class AddStudentDTo
    {
        public string Student_Name { get; set; }
        public string fullName { get; set; }
        public string Student_Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Phone_Number { get; set; }
        [Column(TypeName = "date")]
        public DateTime enrollmentDate { get; set; }
        public string Stage { get; set; }
        public int Level { get; set; }
        public string Parent_ID { get; set; }
    }
}
