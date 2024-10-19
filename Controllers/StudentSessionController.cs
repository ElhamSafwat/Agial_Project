using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentSessionController : ControllerBase
    {
        public AgialContext context;
        public StudentSessionController(AgialContext _context)
        {
            this.context = _context;
        }
        
        [HttpPost("AddAssignmentToSession/{sessionId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddAssignmentToSession(int sessionId,string assignment)
        {
            var session = await context.sessions
                .Include(s => s.Teacher_Class)
                .FirstOrDefaultAsync(s =>  s.Session_ID==sessionId);

            if (session == null)
            {
                return NotFound(new { message = "لا يوجد حصص." });
            }

           
            int classId = session.Teacher_Class.Class_ID.GetValueOrDefault();
            var studentsInClass = await context.student_classes
                .Where(sc => sc.Class_ID == classId)
                .Include(sc => sc.students)
                .ToListAsync();

            if (studentsInClass == null || studentsInClass.Count == 0)
            {
                return NotFound(new { message = "لا يوجد طلاب في هذه المجموعة." });
            }

            foreach (var studentClass in studentsInClass)
            {
                var existingSessionStudent = await context.Session_Students
                    .FirstOrDefaultAsync(ss => ss.Session_ID == sessionId && ss.Student_ID == studentClass.Student_ID);

                    var sessionStudent = new Session_Student
                    {
                        Session_ID = sessionId,
                        Student_ID = studentClass.Student_ID,
                        Assignment = assignment,
                    };
                    context.Session_Students.Add(sessionStudent);
                
            }

            await context.SaveChangesAsync();
            return Ok(new { message = "تم إضافة الواجب للطلاب." });
        }


        
        #region
        [HttpPost("RecordAttendance")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RecordAttendance( List<CreateAttendance> attendances)
        {
            if (attendances == null || !attendances.Any())
            {
                return BadRequest(new { message = "لا توجد بيانات للحضور." });
            }

            try
            {
                var sessionStudents = new List<Session_Student>();

                foreach (var attendance in attendances.Where(a => a.session_id == attendances[0].session_id))
                {
                    var sessionStudent = await context.Session_Students
                        .FirstOrDefaultAsync(ss => ss.Session_ID == attendances[0].session_id && ss.Student_ID == attendance.studentId);

                    if (sessionStudent != null)
                    {
                        // تحديث الحضور
                        sessionStudent.Attendance = attendance.attandence;
                        sessionStudents.Add(sessionStudent);
                    }
                    else
                    {
                        // إضافة سجل جديد إذا لم يكن موجودًا
                        sessionStudents.Add(new Session_Student
                        {
                            Session_ID = attendance.session_id,
                            Student_ID = attendance.studentId,
                            Attendance = attendance.attandence,
                        });
                    }
                }


                //// تحديث أو إضافة السجلات
                if (sessionStudents.Any())
                {
                    context.Session_Students.UpdateRange(sessionStudents);
                    await context.SaveChangesAsync();
                }

                return Ok(new { message = "تم تسجيل الحضور بنجاح." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
        #region add degree
        [HttpPost("AddStudentDegree")]
        public async Task<IActionResult> AddStudentDegree(List<CreatedegreeforAssigment> assignments)
        {
            try
            {
                // جلب جميع سجلات Session_Students من قاعدة البيانات
                var sessionStudents = await context.Session_Students.ToListAsync();

                foreach (var assignment in assignments)
                {
                    // التحقق من أن assignment ليس null
                    if (assignment.assignment == null)
                    {
                        return BadRequest("لا يمكن إضافة درجة لمهمة غير محددة.");
                    }

                    // البحث عن الطالب في القائمة بناءً على الشروط المحددة
                    var studentSession = sessionStudents.FirstOrDefault(item =>
                        item.Student_ID == assignment.studentId &&
                        item.Session_ID == assignment.sessionId &&
                        item.Assignment == assignment.assignment);

                    // إذا وُجد الطالب، قم بتحديث الدرجة
                    if (studentSession != null)
                    {
                        studentSession.Degree = assignment.degree;
                        context.Update(studentSession);
                    }
                    else
                    {
                        return BadRequest("لم يتم العثور على الطالب أو الجلسة أو المهمة المحددة.");
                    }
                }

                // حفظ التغييرات في قاعدة البيانات
                await context.SaveChangesAsync();
                return Ok(new { message = "تم إضافة درجات بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion


        [HttpGet("{teacherid}/{classid}/{date}")]
        public IActionResult GetSisseion(string teacherid,int classid,DateTime date)
        {
            var teclass=context.teacher_Classes.Where(s=>s.Teacher_ID == teacherid && s.Class_ID==classid).Select(s=>s.TC_ID).FirstOrDefault();
            if (teclass == null)
            {
                return NotFound(new { message = "هذا المعلم ليس لديه مجموعه" });

            }
            var sessiond = context.sessions.Where(sd => DateOnly.FromDateTime(sd.Date) == DateOnly.FromDateTime(date) && sd.TC_ID == (int)teclass).Select(s => new {sessionid=s.Session_ID,start=s.Start_Time}).ToList();
            return Ok(sessiond);
        }
        [HttpPost("{teacherid}/{classid}/{date}/{start}")]
        public async Task<IActionResult> addassignment(string teacherid, int classid, DateTime date,float start, string assignment)
        {
            var teclass = context.teacher_Classes.Where(s => s.Teacher_ID == teacherid && s.Class_ID == classid).Select(s => s.TC_ID).FirstOrDefault();
            if (teclass == null)
            {
                return NotFound(new { message = "هذا المعلم ليس لديه مجموعه" });

            }
            var sessiond = context.sessions.Where(sd => DateOnly.FromDateTime(sd.Date) == DateOnly.FromDateTime(date)&&sd.Start_Time==start && sd.TC_ID == (int)teclass).Select(s => s.Session_ID).FirstOrDefault();
            var studentsInClass = await context.student_classes
                .Where(sc => sc.Class_ID == classid)
                .Include(sc => sc.students)
                .ToListAsync();

            if (studentsInClass == null || studentsInClass.Count == 0)
            {
                return NotFound(new { message = "لا يوجد طلاب في هذه المجموعة." });
            }

            foreach (var studentClass in studentsInClass)
            {
                var existingSessionStudent = await context.Session_Students
                    .FirstOrDefaultAsync(ss => ss.Session_ID == sessiond && ss.Student_ID == studentClass.Student_ID);

                var sessionStudent = new Session_Student
                {
                    Session_ID = sessiond,
                    Student_ID = studentClass.Student_ID,
                    Assignment = assignment,
                };
                context.Session_Students.Add(sessionStudent);

            }

            context.SaveChanges();
            return Ok(new { message = "تم إضافة الواجب للطلاب." });
        }
    }
}
