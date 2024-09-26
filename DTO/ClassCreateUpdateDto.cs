namespace final_project_Api.Admin_ClassDTO
{
    public class ClassCreateUpdateDto
    {
       // public int ClassID { get; set; }
        public string Stage { get; set; }
        public string ClassName { get; set; }
        public int Level { get; set; }
        // IDs of Subject to be added to the class
        public List<int> SubjectIds { get; set; }
        // IDs of Students to be added to the class
        public List<string> StudentIds { get; set; }

        // IDs of Teachers to be added to the class
        public List<string> TeacherIds { get; set; }
        
    }
}
