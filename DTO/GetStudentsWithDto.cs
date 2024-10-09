namespace final_project_Api.DTO
{
    public class StudentDTOs
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; } 
    }

    public class ClassWithStudentsDTO
    {
        public string ClassName { get; set; } 
        public List<StudentDTO> Students { get; set; } 
    }
}
