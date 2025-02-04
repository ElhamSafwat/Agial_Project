﻿namespace final_project_Api.DTO
{
    public class getStudentExamDegreeDTO
    {
        public string StudentName { get; set; }
        public string TeacherName { get; set; }
        public int Exam_Id { get; set; }
        public DateOnly Exam_Date { get; set; }
        public float Degree { get; set; }
        public string SubjectName { get; set; }   // New property for Subject Name
        public string ClassName { get; set; }
    }
}
