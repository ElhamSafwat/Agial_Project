﻿namespace final_project_Api.DTO
{
    public class Get_ExamByTeacherAndDateDTO
    {
        public int Exam_ID { get; set; }
        public DateTime Exam_Date { get; set; }
        public float Start_Time { get; set; }
        public float End_Time { get; set; }
        public int Min_Degree { get; set; }
        public int Max_Degree { get; set; }
        public string class_name { get; set; }
        public string subject_name { get; set; }
        public string Teacher_Name { get; set; }
    }
}
