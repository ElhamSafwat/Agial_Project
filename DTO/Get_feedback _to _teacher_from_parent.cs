namespace final_project_Api.DTO
{
    public class Get_feedback__to__teacher_from_parent
    {
        public int Id { get; set; }
      
        public string? Student_Name { get; set; }
        public string? parent_Name { get; set; }
        public string Feedback { get; set; }
        public DateOnly date { get; set; }
    }
}
