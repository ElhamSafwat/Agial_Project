namespace final_project_Api.DTO
{
    public class StudentCalendar
    {
        public int session_id { get; set; }
        public string session_name { get; set; }
        public string room { get; set; }
        public float start { get; set; }
        public float end { get; set; }
        public string period { get; set; }
        public DateOnly date { get; set; }
    }
}
