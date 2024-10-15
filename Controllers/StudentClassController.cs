using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentClassController : ControllerBase
    {
        public AgialContext context;
        public StudentClassController(AgialContext _context)
        {
            context = _context;
        }

        // GET: api/TeacherClass/GetTeachersByStudent/{studentId}
        [HttpGet("GetTeachersByStudent/{studentId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetTeachersByStudent(string studentId)
        {
            var classId = await context.student_classes
                .Where(sc => sc.Student_ID == studentId)
                .Select(sc => sc.Class_ID)
                .FirstOrDefaultAsync();

            if (classId == null)
            {
                return NotFound(new { message = "Student not enrolled in any class." });
            }

            var teachers = await context.teacher_Classes
                .Where(tc => tc.Class_ID == classId)
                .Select(tc => new
                {
                    TeacherId = tc.Teacher.UserId,
                    TeacherName = tc.Teacher.User.Full_Name 
                })
                .Distinct() 
                .ToListAsync();

            if (!teachers.Any())
            {
                return NotFound(new { message = "No teachers found for this student." });
            }

            return Ok(teachers);
        }
    }
}

