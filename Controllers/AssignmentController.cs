using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddAssignmentToSession(int sessionId, string assignment)
        {
            //Find the TC_ID associated with the specified session.
            var session = await agialContext.sessions
                .Include(s => s.Teacher_Class)
                .FirstOrDefaultAsync(s => s.Session_ID == sessionId);

            if (session == null)
            {
                return NotFound(new { message = "Session not found." });
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
                return NotFound(new { message = "No students found in the specified class." });
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

            return Ok(new { message = "Assignment added to all students in the class." });
        }


        [HttpGet("ByClassAndSession/{sessionId}")]
        [Authorize(Roles = "Teacher,Student")]
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
        [Authorize(Roles = "Teacher")]
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
        [Authorize(Roles = "Teacher")]
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
        [Authorize(Roles = "Teacher")]
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
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<AssignmentDTO>>> GetAssignmentsByDate(DateTime date)
        {
            return await agialContext.Session_Students
                .Where(ss => ss.Session.Date == date)
                .Select(ss => new AssignmentDTO
                {
                    SessionID = ss.Session_ID ?? 0,
                    Assignment = ss.Assignment,
                    Session_Date = ss.Session.Date
                }).Distinct()
                .ToListAsync();
        }


        [HttpPost("AddStudentDegree")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddStudentDegree( List<CreatedegreeforAssigment> assigment)
        {

            try
            {
                var sessionStudent = await agialContext.Session_Students.ToListAsync();
                foreach (var ass in assigment)
                {
                    foreach (var item in sessionStudent)
                    {
                        if (ass.studentId == item.Student_ID && ass.sessionId == item.Session_ID)
                        {
                            item.Degree = ass.degree;
                            agialContext.Update(item);
                            break;
                        }
                    }

                }
                agialContext.SaveChanges();
                return Ok(new{ message="تم اضافه درجات بنجاح"});
            }
            catch (Exception ex) { 
            
              return BadRequest(ex.Message);
            }







          
        }
    }
}
