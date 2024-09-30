namespace final_project_Api.DTO
{
    public class StudentTeacherFeedbackAddDto
    {
        public string? Teacher_ID { get; set; }
        public string? Student_ID { get; set; }
        public string Feedback { get; set; }
        public DateTime Date { get; set; }
    }
}
