namespace final_project_Api.DTO
{
    public class Get_for_editClass
    {

        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string Stage { get; set; }
        public int Level { get; set; }

        public List<StudentgetDto> Students { get; set; }

        public List<TeachergetDto> Teachers { get; set; }

        public List<string> SubjectNames { get; set; }
    

    public class StudentgetDto
    {
        public string StudentID { get; set; }
        public string FullName { get; set; }
    }

    public class TeachergetDto
    {
        public string TeacherID { get; set; }
        public string FullName { get; set; }
    }

}
}
