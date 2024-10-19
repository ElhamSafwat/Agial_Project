namespace final_project_Api.DTO
{
    public class CreateAttendance
    {
        public  string studentId { get; set; }
        public int session_id {get; set; }
        //public DateTime date { get; set; }
        public bool attandence { get; set; }=false;
    }
}
