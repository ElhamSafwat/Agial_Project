namespace final_project_Api.DTO
{
    public class teacherparentfeedbackgetall
    {
        public string TeacherName { get; set; }  // اسم المعلم
        public string ParentName { get; set; }   // اسم الوالد
        public string StudentName { get; set; }  // اسم الطالب
        public DateTime Date { get; set; }       // تاريخ الملاحظة
        public string FeedBack { get; set; }     // نص الملاحظة
        public string From { get; set; }



    }
}
