namespace final_project_Api.DTO
{
    public class StudentTeacherFeedbackDto
    {
        public int Student_Teacher_Feedback_Id { get; set; }
        //public string? Teacher_ID { get; set; }
        public string? TeacherName { get; set; }
        public string? StudentName { get; set; }
        //public string? Student_ID { get; set; }
        public string Feedback { get; set; }
        public DateTime Date { get; set; }
    }
}
