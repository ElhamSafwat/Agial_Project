using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Parent_Teacher_Feedback
    {
        [Key]
        public int Parent_Teacher_Feedback_Id { get; set; }
        [ForeignKey("Teacher")]
        public string? Teacher_ID { get; set; } 
        public virtual Teacher? Teacher { get; set; }
        [ForeignKey("Parent")]
        public string? Parent_ID { get; set; } 
        public virtual Parent? Parent { get; set; }

        public string FeedBack { get; set; }
        public string From { get; set; }

    }
}
