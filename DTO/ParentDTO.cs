namespace final_project_Api.DTO
{
    public class ParentDTO
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public List<Create_StudentWithParent> Students { get; set; }
    }
}
