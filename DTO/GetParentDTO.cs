namespace final_project_Api.Parentdtos
{
    public class GetParentDTO
    {
        public string? UserId { get; set; }
        public string FullName { get; set; }  // Corresponds to ApplicationUser.Full_Name
        public string Fullphone { get; set; }  // Corresponds to ApplicationUser.Full_Name
        public string? Email { get; set; }  // Corresponds to ApplicationUser.Full_Name
        public List<string> Studentname { get; set; }  // List of Student UserIds

        //    public string? UserId { get; set; }
        //    public string FullName { get; set; }  // Corresponds to ApplicationUser.Full_Name
        //    public string phoneName { get; set; }  // Corresponds to ApplicationUser.Full_Name
        //    public string Email { get; set; }  // Corresponds to ApplicationUser.Full_Name
        //    public List<string> Studentname { get; set; }
        //}

    }
}
