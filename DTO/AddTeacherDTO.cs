using System.ComponentModel.DataAnnotations;

namespace final_project_Api.DTOs
{
    public class AddTeacherDTO
    {
        public string UserName { get; set; }
        public string Full_Name { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; } 
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [RegularExpression("^(أبتدائي|أعدادي|ثانوي)$", ErrorMessage = "القيمه يجب ان تكون أبتدائي او اعدادي او ثانوي")]
        public string Stage { get; set; }
        public int Subject_ID { get; set; } // Optional: to associate with a subject
        public DateTime HireDate { get; set; }
    }
}
