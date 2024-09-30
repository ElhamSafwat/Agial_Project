namespace final_project_Api.DTOs
{
    public class TeacherWithSubjectDTO
    {
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public DateTime HireDate { get; set; }
        public string? phoneNumber {  get; set; }
        public string stage { get; set; }
        public bool IsDelete { get; set; }
    }
}
