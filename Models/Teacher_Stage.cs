using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.Models
{
    public class Teacher_Stage
    {
        [Key]
        public int Teacher_Stage_Id { get; set; }
        [RegularExpression("^(أبتدائي|أعدادي|ثانوي)$", ErrorMessage = "القيمه يجب ان تكون أبتدائي او اعدادي او ثانوي")]
        public string Stage { get; set; }
        [ForeignKey("teacher")]
        public string Teacher_Id { get; set; }
        public virtual Teacher teacher { get; set; }
    }
}
