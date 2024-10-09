using final_project_Api.Admin_ClassDTO;
using final_project_Api.DTO;
using final_project_Api.DTOs;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class teacher_Classes : ControllerBase
    {
        private readonly AgialContext _context;

        public teacher_Classes(AgialContext context)
        {
            _context = context;
        }



        [HttpGet("{teacherId}/classes")]
        public IActionResult GetClassesByTeacherId(string teacherId)
        {
            var teacherClasses = _context.teacher_Classes
                .Where(tc => tc.Teacher_ID == teacherId)
                .Include(tc => tc.Class)
                .Select(tc => tc.Class.Class_Name)
                .Distinct() 
                .ToList();

            if (teacherClasses.Count == 0)
            {
                return NotFound("No classes found for this teacher.");
            }

            var result = new GetTeacherClass
            {
                classname = teacherClasses
            };

            return Ok(result);
        }





    }
}
