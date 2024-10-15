using final_project_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentTeacherClassController : ControllerBase
    {
        private readonly AgialContext _context;

        public StudentTeacherClassController(AgialContext context)
        {
            _context = context;
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<List<object>>> GetTeachersByStudentId(string studentId)
        {
            var classIds = await _context.student_classes
                .Where(sc => sc.Student_ID == studentId)
                .Select(sc => sc.Class_ID)
                .ToListAsync();

            if (classIds == null || !classIds.Any())
            {
                return NotFound("No classes found for the student.");
            }

            var teacherIds = await _context.teacher_Classes
                .Where(tc => classIds.Contains(tc.Class_ID))
                .Select(tc => tc.Teacher_ID)
                .Distinct()
                .ToListAsync();

            if (teacherIds == null || !teacherIds.Any())
            {
                return NotFound("No teachers found for the student's classes.");
            }

            // الحصول على معرفات وأسماء المعلمين
            var teacherDetails = await _context.teachers
                .Where(t => teacherIds.Contains(t.UserId))
                .Select(t => new
                {
                    TeacherID = t.UserId,
                    FullName = t.User.Full_Name
                })
                .ToListAsync();

            return Ok(teacherDetails);
        }
    }
}
