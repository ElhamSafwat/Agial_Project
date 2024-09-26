namespace WebAPIDotNet.DTO
{
    public class RegisterDto
    {
        public string Role { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone_Number { get; set; }
        public DateTime enrollmentDate { get; set; }
        public string Parent_ID { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsDelete { get; set; }
        public int Subject_ID { get; set; }
    }
}
