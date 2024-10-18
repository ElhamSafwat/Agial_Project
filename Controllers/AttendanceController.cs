using final_project_Api.Admin_ClassDTO;
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
    public class AttendanceController : ControllerBase
    {
        private readonly AgialContext agialContext;
        public AttendanceController(AgialContext _agialContext)
        {
            agialContext = _agialContext;
        }
        #region attendance
        [HttpPost("RecordAttendance")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RecordAttendance(List<CreateAttendance> attendances)
        {
            if (attendances == null || attendances.Count == 0)
            {
                return BadRequest(new { message = "لا يوجد بيانات لحضور الطلاب." });
            }

            List<Session_Student> list = new List<Session_Student>();

            try
            {
                foreach (var attendance in attendances)
                {
                    var sessionStudent = new Session_Student
                    {
                        Session_ID = attendance.session_id,
                        Student_ID = attendance.studentId,
                        Attendance = attendance.attandence
                    };
                    list.Add(sessionStudent);
                }

                 agialContext.Session_Students.AddRange(list);
               agialContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return Ok(new { message = "تم تسجيل / تحديث الحضور للطلاب في الحصة." });
        }

        #endregion
       

        #region GetAll
        // GET: api/Attendance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionStudentDTO>>> GetSessionStudents()
        {
            // Map the Entity to DTO
            return await agialContext.Session_Students
                .Select(ss => new SessionStudentDTO
                {
                    SessionID = ss.Session_ID,
                    Student_ID = ss.Student_ID,
                    Attendance = ss.Attendance
                })
                .ToListAsync();
        }
        #endregion

        #region Get Students by Class
        // GET: api/Attendance/GetStudentsByClass/5
        [HttpGet("GetStudentsByClass/{classId}")]
        public async Task<ActionResult<IEnumerable<StudentAttendanceDTO>>> GetStudentsByClass(int classId)
        {
            var students = await agialContext.student_classes
                .Where(sc => sc.Class_ID == classId)
                .Join(agialContext.students,
                    sc => sc.Student_ID,
                    s => s.UserId,
                    (sc, s) => new { sc, s })
                .GroupJoin(agialContext.Session_Students, // Join with attendance records
                    combined => combined.sc.Student_ID,
                    ss => ss.Student_ID,
                    (combined, sessions) => new
                    {
                        combined.sc,
                        combined.s,
                        Attendance = sessions.Select(session => session.Attendance).FirstOrDefault()
                    })
                .Select(result => new StudentAttendanceDTO
                {
                    UserID = result.sc.Student_ID,
                    Full_Name = result.s.User != null ? result.s.User.Full_Name : "غير متوفر",
                    Attendance = result.Attendance
                })
                .ToListAsync();

            if (students == null || !students.Any())
            {
                return NotFound(new { message = "لا يوجد طلاب في هذا الفصل " });
            }

            return students;
        }
        #endregion



        // GET: api/Attendance/5
        [HttpGet("{sessionId}/{studentId}")]
       
        public async Task<ActionResult<SessionStudentDTO>> GetSessionStudent(int sessionId, string studentId)
        {
            var sessionStudent = await agialContext.Session_Students
                .Where(ss => ss.Session_ID == sessionId && ss.Student_ID == studentId)
                .Select(ss => new SessionStudentDTO
                {
                    SessionID = ss.Session_ID,
                    Student_ID = ss.Student_ID,
                    Attendance = ss.Attendance
                })
                .FirstOrDefaultAsync();

            if (sessionStudent == null)
            {
                return NotFound(new { message = "لا يوجد" });
            }

            return sessionStudent;
        }

       
        // PUT: api/Attendance/5
        [HttpPut("{sessionId}/{studentId}")]
        
        public async Task<IActionResult> PutSessionStudent(int sessionId, string studentId, SessionStudentDTO sessionStudentDTO)
        {
            if (sessionId != sessionStudentDTO.SessionID || studentId != sessionStudentDTO.Student_ID)
            {
                return BadRequest(new { message = "Session ID or Student ID does not match." });
            }


            var sessionStudent = await agialContext.Session_Students
                .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId && ss.Student_ID == studentId);

            if (sessionStudent == null)
            {
                return NotFound(new { message = "Student in the specified session not found." });
            }


            sessionStudent.Attendance = sessionStudentDTO.Attendance;


            try
            {
                agialContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionStudentExists(sessionId, studentId))
                {
                    return NotFound(new { message = "Concurrency issue: Student in the session not found." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool SessionStudentExists(int sessionId, string studentId)
        {
            return agialContext.Session_Students.Any(ss => ss.Session_ID == sessionId && ss.Student_ID == studentId);
        }
    }
}