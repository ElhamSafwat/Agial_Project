namespace final_project_Api.DTO
{
    public class Create_Session
    {

        public DateTime Date { get; set; }
       
        public string Room { get; set; }
        public float End_Time { get; set; }
        public float Start_Time { get; set; }
        public string period { get; set; }
        public string Teacher_ID { get; set; }
        public int Class_ID { get; set; }
    }
}
