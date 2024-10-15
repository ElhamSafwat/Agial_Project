namespace final_project_Api.DTO
{
    public class showattendanceforadminDTO
    {
        public string studentname {  get; set; }
        public string teachername {  get; set; }
        public string subjectname {  get; set; }
        public string classname {  get; set; }
        public DateOnly sessiondate { get; set; }
        public string attendance { get; set; }
    }
}
