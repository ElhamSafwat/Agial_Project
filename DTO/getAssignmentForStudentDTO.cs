namespace final_project_Api.DTO
{
    public class getAssignmentForStudentDTO
    {
        public int? Session_ID { get; set; }
        public string studentId { get; set; }

        public DateOnly Date { get; set; }
        public string Assignment { get; set; }
        public float Degree { get; set; }
        public string subject_Name { get; set; }

    }
}
