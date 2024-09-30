namespace final_project_Api.DTO
{
    public class Get_Parent_Teacher_Feedback
    {
        public int Id { get; set; }
        public string? Teacher_Name { get; set; }
        public string? Student_Name { get; set; }
        public string? parent_Name { get; set; }
        public string Feedback { get; set; }
        public DateOnly date { get; set; }
    }
}
