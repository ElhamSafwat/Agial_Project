namespace final_project_Api.DTO
{
    public class Create_StudentWithParent
    {
        public string Student_Name { get; set; }
        public string fullName { get; set; }
        public string Student_Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Phone_Number { get; set; }
        public DateTime enrollmentDate { get; set; }
        public string Stage { get; set; }
        public int Level { get; set; }
    }
}
