﻿namespace final_project_Api.DTO
{
    public class StudentExamResultForTeacherDto
    {
        public string StudentName { get; set; }
        public string TeacherName { get; set; }
        public int StudentID { get; set; }  
        public int Exam_Id { get; set; }
        public float Degree { get; set; }
        public float MaxDegree { get; set; } 
        public string SubjectName { get; set; }   
        public string ClassName { get; set; }
    }
}
