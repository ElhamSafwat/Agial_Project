namespace final_project_Api.Admin_ClassDTO
{
    public class ClassDto
    {
        public int ClassID { get; set; }
        public string Stage { get; set; }
        public string ClassName { get; set; }
        public int Level { get; set; }

        public List<string> StudentNames { get; set; } // List of student names
        public List<string> TeacherNames { get; set; } // List of teacher names
        public List<string> SubjectNames { get; set; } // List of Subject Names 
    }
}
