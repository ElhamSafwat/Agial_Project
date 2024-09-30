namespace final_project_Api.DTO
{
    public class Create_TeacherparentFeedback
    {
        public string Teacher_ID { get; set; }  
        public string Parent_ID { get; set; }   
        public string Student_ID { get; set; }  
        public string FeedBack { get; set; }   
        public DateTime FeedbackDate { get; set; }  
    }
}
