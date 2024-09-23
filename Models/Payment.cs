using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace final_project_Api.Models
{
    public class Payment
    {
        [Key]
        public int Payment_ID { get; set; }
        [Column(TypeName ="date")]
        public DateTime Date { get; set; }
        public string State { get; set; }
        public float Amount { get; set; }
        public string Payment_Method { get; set; }
        [ForeignKey("Student")]
        public string? Student_ID { get; set; }
        public virtual Student? Student { get; set; }
        [ForeignKey("admin")]
        public string? Admin_ID { get; set; }
        public virtual Admin? admin { get; set; }
    }
}
