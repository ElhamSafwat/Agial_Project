﻿namespace final_project_Api.DTOs
{
    public class getsubjectDto
    {

        public string Subject_Name { get; set; }

        public string Description { get; set; }
        public List<string> TeacherNames { get; set; } = new List<string>(); 


    }
}