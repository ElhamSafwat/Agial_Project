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


        //[HttpPost("AddAssignmentToSession/{sessionId}/{assignment}")]
        //[HttpPost("AddAssignmentToSession/{sessionId}")]

        //[Authorize(Roles = "Teacher")]
        //public async Task<IActionResult> AddAssignmentToSession(int sessionId, string assignment)
        //{
        //    //Find the TC_ID associated with the specified session.
        //    var session = await agialContext.sessions
        //        .Include(s => s.Teacher_Class)
        //        .FirstOrDefaultAsync(s => s.Session_ID == sessionId && s.Teacher_Class.TC_ID==s.TC_ID);

        //    if (session == null)
        //    {
        //        return NotFound(new { message = "لا يوجد حصص " });
        //    }

        //    // Class_ID from Teacher_Class
        //    int classId = session.Teacher_Class.Class_ID.GetValueOrDefault();

        //    // Bring all students associated with the specified class.
        //    var studentsInClass = await agialContext.student_classes
        //        .Where(sc => sc.Class_ID == classId)
        //        .Include(sc => sc.students)
        //        .ToListAsync();

        //    if (studentsInClass == null || studentsInClass.Count == 0)
        //    {
        //        return NotFound(new { message = "لا يوجد طلاب في هذه المجموعه " });
        //    }

        //    // إضافة Assignment جديد لكل طالب في الفصل
        //    foreach (var studentClass in studentsInClass)
        //    {
        //        var sessionStudent = new Session_Student
        //        {
        //            Session_ID = sessionId,
        //            Student_ID = studentClass.Student_ID,
        //            Assignment = assignment,
        //        };

        //        agialContext.Session_Students.Add(sessionStudent);
        //    }

        //    // Save changes
        //    await agialContext.SaveChangesAsync();

        //    return Ok(new { message = "تم اضافه الواجب لجميع الطلاب في هذه المجموعه" });
        //}
        #region add assignment
        [HttpPost("AddAssignmentToSession/{sessionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddAssignmentToSession(int sessionId, string assignment)
        {
            var session = await agialContext.sessions
                .Include(s => s.Teacher_Class)
                .FirstOrDefaultAsync(s => s.Session_ID == sessionId);

            if (session == null)
            {
                return NotFound(new { message = "لا يوجد حصص." });
            }

            int classId = session.Teacher_Class.Class_ID.GetValueOrDefault();
            var studentsInClass = await agialContext.student_classes
                .Where(sc => sc.Class_ID == classId)
                .Include(sc => sc.students)
                .ToListAsync();

            if (studentsInClass == null || studentsInClass.Count == 0)
            {
                return NotFound(new { message = "لا يوجد طلاب في هذه المجموعة." });
            }

            foreach (var studentClass in studentsInClass)
            {
                var existingSessionStudent = await agialContext.Session_Students
                    .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId && ss.Student_ID == studentClass.Student_ID);

                if (existingSessionStudent != null)
                {
                    // Update the existing record with the assignment
                    existingSessionStudent.Assignment = assignment;
                    agialContext.Update(existingSessionStudent);
                }
                else
                {
                    // Add a new record if it doesn't exist
                    var sessionStudent = new Session_Student
                    {
                        Session_ID = sessionId,
                        Student_ID = studentClass.Student_ID,
                        Assignment = assignment,
                    };
                    agialContext.Session_Students.Add(sessionStudent);
                }
            }

            agialContext.SaveChanges();
            return Ok(new { message = "تم إضافة الواجب للطلاب." });
        }

        #endregion

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
                return NotFound(new {message="لا يوجد واجب لهذه الحصه و هذا الطالب "});
            }

            // Update Assignment
            sessionStudent.Assignment = assignment;

            // save changes 
            agialContext.SaveChanges();

            return Ok(new {message="تم التعديل بنجاح "});
        }


        [HttpDelete("{sessionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteAssignment(int sessionId)
        {
            var sessionStudent = await agialContext.Session_Students
                .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId);

            if (sessionStudent == null)
            {
                return NotFound(new {message="لا يوجد واجب لهذه الحصه "});
            }

            // Delete Assignment
            sessionStudent.Assignment = null;

            // Save Changes
            agialContext.SaveChanges();

            return Ok(new {message="تم الحذف بنجاح "});
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

        #region get assinment by classid,date
        [HttpGet("ByDate/{classId}/{date}")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<AssignmentDTO>>> GetAssignmentsByDate(int classId, DateTime date)
        {
            // استخرج التاريخ بدون الوقت لمقارنة دقيقة
            var dateOnly = date.Date;

            var assignments = await agialContext.Session_Students
                .Where(ss => ss.Session.Date.Date == dateOnly && ss.Session.Teacher_Class.Class_ID == classId) // تحقق من classId
                .Select(ss => new AssignmentDTO
                {
                    SessionID = ss.Session_ID ?? 0,
                    Assignment = ss.Assignment,
                    Session_Date = ss.Session.Date
                })
                .Distinct()
                .ToListAsync();

            if (assignments == null || !assignments.Any())
            {
                return NotFound(new { message = "لا توجد واجبات مسجلة لهذا التاريخ." });
            }

            return Ok(assignments);
        }

        #endregion

        #region   add degree
        [HttpPost("AddStudentDegree")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddStudentDegree(List<CreatedegreeforAssigment> assignments)
        {
            if (assignments == null || assignments.Count == 0)
            {
                return BadRequest(new { message = "لا توجد درجات لإضافتها." });
            }

            try
            {
                foreach (var ass in assignments)
                {
                    var sessionStudent = await agialContext.Session_Students
                        .FirstOrDefaultAsync(ss => ss.Student_ID == ass.studentId
                                                    && ss.Session_ID == ass.sessionId
                                                    && ss.Assignment == ass.assignment);
                    if (sessionStudent != null)
                    {
                        sessionStudent.Degree = ass.degree;
                        agialContext.Update(sessionStudent);
                    }
                }

                agialContext.SaveChanges();
                return Ok(new { message = "تم إضافة درجات بنجاح." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        //[HttpPost("AddStudentDegree")]
        //public async Task<IActionResult> AddStudentDegree(List<CreatedegreeforAssigment> assignments)
        //{
        //    try
        //    {
        //        // جلب جميع سجلات Session_Students من قاعدة البيانات
        //        var sessionStudents = await agialContext.Session_Students.ToListAsync();

        //        foreach (var assignment in assignments)
        //        {
        //            // التحقق من أن assignment ليس null
        //            if (assignment.assignment == null)
        //            {
        //                return BadRequest("لا يمكن إضافة درجة لمهمة غير محددة.");
        //            }

        //            // البحث عن الطالب في القائمة بناءً على الشروط المحددة
        //            var studentSession = sessionStudents.FirstOrDefault(item =>
        //                item.Student_ID == assignment.studentId &&
        //                item.Session_ID == assignment.sessionId &&
        //                item.Assignment == assignment.assignment);

        //            // إذا وُجد الطالب، قم بتحديث الدرجة
        //            if (studentSession != null)
        //            {
        //                studentSession.Degree = assignment.degree;
        //                agialContext.Update(studentSession);
        //            }
        //            else
        //            {
        //                return BadRequest("لم يتم العثور على الطالب أو الجلسة أو المهمة المحددة.");
        //            }
        //        }

        //        // حفظ التغييرات في قاعدة البيانات
        //        await agialContext.SaveChangesAsync();
        //        return Ok(new { message = "تم إضافة درجات بنجاح" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


    }
}
