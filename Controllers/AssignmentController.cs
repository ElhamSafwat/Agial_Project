using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly AgialContext agialContext;
        public AssignmentController(AgialContext _agialContext)
        {
            agialContext = _agialContext;
        }


        [HttpPost("AddAssignmentToSession/{sessionId}")]
        public async Task<IActionResult> AddAssignmentToSession(int sessionId, string assignment)
        {
            //Find the TC_ID associated with the specified session.
            var session = await agialContext.sessions
                .Include(s => s.Teacher_Class)
                .FirstOrDefaultAsync(s => s.Session_ID == sessionId);

            if (session == null)
            {
                return NotFound("Session not found.");
            }

            // Class_ID from Teacher_Class
            int classId = session.Teacher_Class.Class_ID.GetValueOrDefault();

            // Bring all students associated with the specified class.
            var studentsInClass = await agialContext.student_classes
                .Where(sc => sc.Class_ID == classId)
                .Include(sc => sc.students)  
                .ToListAsync();

            if (studentsInClass == null || studentsInClass.Count == 0)
            {
                return NotFound("No students found in the specified class.");
            }

            // إضافة Assignment جديد لكل طالب في الفصل
            foreach (var studentClass in studentsInClass)
            {
                var sessionStudent = new Session_Student
                {
                    Session_ID = sessionId, 
                    Student_ID = studentClass.Student_ID, 
                    Assignment = assignment,
                };

                agialContext.Session_Students.Add(sessionStudent);
            }

            // Save changes
            await agialContext.SaveChangesAsync();

            return Ok("Assignment added to all students in the class.");
        }


        [HttpGet("ByClassAndSession/{sessionId}")]
        public async Task<ActionResult<IEnumerable<AssignmentDTO>>> GetAssignmentsByClassAndSession( int sessionId)
        {
            // Find all Assignments based on SessionID and ClassID
            return await agialContext.Session_Students
                .Where(ss => ss.Session_ID == sessionId)
                .Select(ss => new AssignmentDTO
                {
                    SessionID = ss.Session_ID ?? 0,  
                    Assignment = ss.Assignment,       
                    Session_Date = ss.Session.Date         
                })
                .ToListAsync();
        }


        [HttpPut("{sessionId}/{studentId}")]
        public async Task<IActionResult> UpdateAssignment(int sessionId, string studentId, string assignment)
        {
            // Search for a session using session number and student number
            var sessionStudent = await agialContext.Session_Students
                .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId && ss.Student_ID == studentId);

            if (sessionStudent == null)
            {
                return NotFound();
            }

            // Update Assignment
            sessionStudent.Assignment = assignment;

            // save changes 
            await agialContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteAssignment(int sessionId)
        {
            var sessionStudent = await agialContext.Session_Students
                .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId);

            if (sessionStudent == null)
            {
                return NotFound();
            }

            // Delete Assignment
            sessionStudent.Assignment = null;

            // Save Changes
            await agialContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentDTO>>> GetAllAssignments()
        {
            return await agialContext.Session_Students
                .Select(ss => new AssignmentDTO
                {
                    SessionID = ss.Session_ID ?? 0,  
                    Assignment = ss.Assignment,
                    Session_Date = ss.Session.Date 
                })
                .ToListAsync();
        }

        [HttpGet("ByDate/{date}")]
        public async Task<ActionResult<IEnumerable<AssignmentDTO>>> GetAssignmentsByDate(DateTime date)
        {
            return await agialContext.Session_Students
                .Where(ss => ss.Session.Date == date)
                .Select(ss => new AssignmentDTO
                {
                    SessionID = ss.Session_ID ?? 0,
                    Assignment = ss.Assignment,
                    Session_Date = ss.Session.Date
                })
                .ToListAsync();
        }


        [HttpPost("AddStudentDegree")]
        public async Task<IActionResult> AddStudentDegree(int sessionId, string studentId, int degree)
        {
            // البحث عن السجل المرتبط بالطالب والجلسة المحددة
            var sessionStudent = await agialContext.Session_Students
                .Where(ss => ss.Session_ID == sessionId && ss. Student_ID == studentId)
                .FirstOrDefaultAsync();

            if (sessionStudent == null)
            {
                return NotFound("Student not found in the specified session.");
            }

            // التحقق من الدرجة المدخلة
            if (degree < 0 || degree >= 10) // بافتراض أن الدرجة تكون من 0 إلى 10
            {
                return BadRequest("Degree must be between 0 and 10.");
            }

            // تعيين الدرجة
            sessionStudent.Degree = degree;
 
            if (degree >= 5)
            {
                Console.WriteLine($"Student {studentId} has successfully completed the assignment.");
            }
            else
            {
                Console.WriteLine($"Student {studentId} did not complete the assignment successfully. Please follow up.");
            }

            await agialContext.SaveChangesAsync();

            return Ok($"Degree {degree} added for student {studentId} in session {sessionId}.");
        }
    }
}
