using final_project_Api.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project_Api.DTO
{
    public class Get_Sessions
    {
        public int? Session_ID { get; set; }
       
        public string? Session_Title { get; set; }
        
        public DateTime Date { get; set; }
        public string Room { get; set; }
        public float End_Time { get; set; }
        public float Start_Time { get; set; }
        public string period { get; set; }
        //public int TC_ID { get; set; }
        //public string Teacher_ID { get; set; }
        //public int Class_ID { get; set; }
        public string? Teacher_Name { get;set; }
        public string? Class_Name { get; set; }

    }
}
