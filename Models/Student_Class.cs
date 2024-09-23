using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace final_project_Api.Models
{
    public class Student_Class
    {
        [Key]
        public int Student_Class_Id { get; set; }
        [ForeignKey(" students")]
        public string? Student_ID {  get; set; }
        public virtual Student? students { get; set; }
        [ForeignKey("classs")]
        public int? Class_ID {  get; set; }
        public virtual Class? classs { get; set; }

    }
}
