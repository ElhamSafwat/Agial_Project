using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Session
    {
        [Key]
        public int Session_ID { get; set; }
        public string Material_Name { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public string Room { get; set; }
        public float End_Time { get; set; }
        public float Start_Time { get; set; }
        public string period { get; set; }
        [ForeignKey("Teacher_Class")]
        public int? TC_ID { get; set; }  
        public virtual Teacher_Class? Teacher_Class { get; set; }  
        public virtual ICollection<Session_Student>? session_Students { get; set; }
    }
}
