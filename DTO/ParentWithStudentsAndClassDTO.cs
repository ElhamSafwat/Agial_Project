namespace final_project_Api.DTO
{
    public class StudentDTO
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; } // Add Student ID
    }

    public class ParentWithStudentsDTO
    {
        public string ParentName { get; set; }
        public string ParentId { get; set; } // Add Parent ID
        public List<StudentDTO> Students { get; set; }
    }

    public class ClassWithStudentsAndParentsDTO
    {
        public string ClassName { get; set; }
        public List<ParentWithStudentsDTO> ParentsWithStudents { get; set; }
    }
}
