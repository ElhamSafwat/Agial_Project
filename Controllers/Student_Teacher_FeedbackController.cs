using final_project_Api.DTO;
using final_project_Api.Models;
using final_project_Api.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Student_Teacher_FeedbackController : ControllerBase
    {
        private readonly AgialContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailFeedback emailFeedback;
        public Student_Teacher_FeedbackController(AgialContext context, UserManager<ApplicationUser> userManager, IEmailFeedback _emailFeedback)
        {
            _context = context;
            _userManager = userManager;
            emailFeedback = _emailFeedback;
        }
        
        //عشان اشوف في نفس الاكلاس ولا
        private bool AreStudentAndTeacherInSameClass(string studentId, string teacherId)
        {
            // التحقق من أن الطالب والمدرس مسجلين في نفس الفصل
            var studentClassIds = _context.student_classes
                                          .Where(sc => sc.Student_ID == studentId)
                                          .Select(sc => sc.Class_ID)
                                          .ToList();

            var teacherClassIds = _context.teacher_Classes
                                          .Where(tc => tc.Teacher_ID == teacherId)
                                          .Select(tc => tc.Class_ID)
                                          .ToList();

            return studentClassIds.Intersect(teacherClassIds).Any();
        }
        [HttpPost("add-feedback")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddFeedback(StudentTeacherFeedbackAddDto dto)
        {
            // التحقق من وجود الطالب
            var student = await _context.students.FindAsync(dto.Student_ID);
            var teacher = await _context.teachers.FindAsync(dto.Teacher_ID);

            // التحقق مما إذا كان الطالب أو المدرس غير موجود
            if (student == null && teacher == null)
            {
                return NotFound(new{message="Both student and teacher not found in the database."});
            }
            if (student == null)
            {
                return NotFound(new { message= "Student not found ." });
            }
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found." });
            }

            // التحقق مما إذا كان الطالب والمدرس في نفس الفصل
            if (!AreStudentAndTeacherInSameClass(dto.Student_ID, dto.Teacher_ID))
            {
                return BadRequest(new { message = "The student and teacher are not in the same class." });
            }

            // إضافة الملاحظة بعد التحقق من صحة البيانات
            var feedback = new Student_Teacher_Feedback
            {
                Student_ID = dto.Student_ID,
                Teacher_ID = dto.Teacher_ID,
                Feedback = dto.Feedback,
                date = DateTime.Now // استخدام التاريخ الحالي أثناء الإضافة
            };

            _context.student_Teacher_Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback added successfully." });
        }
        //############################################

        [HttpPut("update-feedback/{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, UpdateStudent_Teacher_FeedbackDTO dto)
        {
            var feedback = await _context.student_Teacher_Feedbacks.FindAsync(id);

            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found." });
            }

            //if (!AreStudentAndTeacherInSameClass(dto.Student_ID, dto.Teacher_ID))
            //{
            //    return BadRequest("The student and teacher are not in the same class.");
            //}

            feedback.Feedback = dto.Feedback;
            feedback.date = DateTime.Now;
            _context.student_Teacher_Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback updated successfully." });
        }

        //[HttpGet("get-all-feedbacks")]
        //public async Task<IActionResult> GetAllFeedbacks()
        //        {
        //            var feedbacks = await _context.student_Teacher_Feedbacks
        //                .Include(f => f.Student)
        //                .Include(f => f.Teacher)
        //                .ToListAsync();

        //            return Ok(feedbacks);
        //        }
        //            [HttpGet("get-feedback/{id}")]
        //            public async Task<IActionResult> GetFeedbackById(int id)
        //            {
        //                var feedback = await _context.student_Teacher_Feedbacks
        //                    .Include(f => f.Student)
        //                    .Include(f => f.Teacher)
        //                    .FirstOrDefaultAsync(f => f.Student_Teacher_Feedback_Id == id);

        //                if (feedback == null)
        //                {
        //                    return NotFound("Feedback not found.");
        //                }

        //                return Ok(feedback);
        //            }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<StudentTeacherFeedbackDto>>> GetAllFeedbacks()
        {
            var feedbacks = await _context.student_Teacher_Feedbacks
                                           .Include(f => f.Teacher)
                                           .Include(f => f.Student)
                                           .Select(f => new StudentTeacherFeedbackDto
                                           {
                                               Student_Teacher_Feedback_Id = f.Student_Teacher_Feedback_Id,
                                               //Teacher_ID = f.Teacher_ID,
                                               //Student_ID = f.Student_ID,
                                               StudentName = f.Student.User.Full_Name,  // Assuming the `ApplicationUser` class has `FullName` property
                                               TeacherName = f.Teacher.User.Full_Name,
                                               Feedback = f.Feedback,
                                               Date = f.date
                                           }).ToListAsync();

            return Ok(feedbacks);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentTeacherFeedbackDto>> GetFeedbackById(int id)
        {
            var feedback = await _context.student_Teacher_Feedbacks
                                         .Include(f => f.Teacher)
                                         .Include(f => f.Student)
                                         .Where(f => f.Student_Teacher_Feedback_Id == id)
                                         .Select(f => new StudentTeacherFeedbackDto
                                         {
                                             Student_Teacher_Feedback_Id = f.Student_Teacher_Feedback_Id,
                                             //Teacher_ID = f.Teacher_ID,
                                             //Student_ID = f.Student_ID,
                                             Feedback = f.Feedback,
                                             Date = f.date
                                         }).FirstOrDefaultAsync();

            if (feedback == null)
            {
                return NotFound(new { message = "feedback not found" });
            }
            return Ok(feedback);
        }
        [HttpGet("get-feedbacks-by-teacher/{teacherId}")]
        
        public async Task<IActionResult> GetFeedbacksByTeacher(string teacherId)
            {
                var feedbacks = await _context.student_Teacher_Feedbacks
                    .Where(f => f.Teacher_ID == teacherId)
                    .Include(f => f.Student)
                    .Include(f => f.Teacher)
                    .ToListAsync();

                return Ok(feedbacks);
            }
        [HttpGet("get-feedbacks-by-student/{studentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetFeedbacksByStudent(string studentId)
        {
            var feedbacks = await _context.student_Teacher_Feedbacks
                .Where(f => f.Student_ID == studentId)
                .Include(f => f.Student)
                .ThenInclude(s => s.User) 
                .Include(f => f.Teacher)
                .ThenInclude(t => t.User) // 
                .Select(f => new StudentTeacherFeedbackDto
                {
                    Student_Teacher_Feedback_Id = f.Student_Teacher_Feedback_Id,
                    //Teacher_ID = f.Teacher_ID,
                    //Student_ID = f.Student_ID,
                    StudentName = f.Student.User.Full_Name,
                    TeacherName = f.Teacher.User.Full_Name,  // تأكد من تحميل الاسم من المدرس المرتبط
                    Feedback = f.Feedback,
                     Date= f.date
                })
                .ToListAsync();

            if (feedbacks == null || feedbacks.Count == 0)
            {
                return NotFound(new { message = "No feedbacks found for this student." });
            }

            return Ok(feedbacks);
        }
        [HttpGet("get-feedbacks-by-class/{classId}")]
        public async Task<IActionResult> GetFeedbacksByClass(int classId)
        {
            // التحقق من أن الطلاب والمدرسين ينتمون إلى نفس الفصل
            var feedbacks = await _context.student_Teacher_Feedbacks
                .Where(f => _context.student_classes
                    .Any(sc => sc.Student_ID == f.Student_ID && sc.Class_ID == classId) &&
                    _context.teacher_Classes
                    .Any(tc => tc.Teacher_ID == f.Teacher_ID && tc.Class_ID == classId))
                .Include(f => f.Student)
                .ThenInclude(s => s.User) // لتحميل بيانات الطالب مثل الاسم
                .Include(f => f.Teacher)
                 .ThenInclude(t => t.User) // لتحميل بيانات المدرس مثل الاسم
                .Select(f => new StudentTeacherFeedbackDto
                {
                        Student_Teacher_Feedback_Id = f.Student_Teacher_Feedback_Id,
                    StudentName = f.Student.User.Full_Name,
                    TeacherName = f.Teacher.User.Full_Name,
                    Feedback = f.Feedback,
                    Date = f.date
                })
                .ToListAsync();

            if (feedbacks == null || feedbacks.Count == 0)
            {
                return NotFound(new { message = "No feedbacks found for this class." });
            }

            return Ok(feedbacks);
        }
        //##############################################
        [HttpGet("{className}/{stage}/{level}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFeedbacksByClassInfo(string className, string stage, int level)
        {
            // التحقق مما إذا كان اسم الفصل موجود في قاعدة البيانات
            var classExists = await _context.classes
                .FirstOrDefaultAsync(c => c.Class_Name == className);

            // التحقق مما إذا كانت المرحلة موجودة في قاعدة البيانات
            var stageExists = await _context.classes
                .FirstOrDefaultAsync(s => s.Stage == stage);

            // التحقق مما إذا كان المستوى موجودًا في قاعدة البيانات
            var levelExists = await _context.classes
                .FirstOrDefaultAsync(l => l.Level == level);

            // حالة التحقق من وجود الثلاثة معًا
            if (classExists == null && stageExists == null && levelExists == null)
            {
                return NotFound(new { message = "Class name, stage, and level do not exist." });
            }

            // إذا كان اسم الفصل غير موجود
            if (classExists == null)
            {
                return NotFound(new { message = "Class name does not exist." });
            }

            // إذا كانت المرحلة غير موجودة
            if (stageExists == null)
            {
                return NotFound(new { mewwsage = "Stage name does not exist." });
            }

            // إذا كان المستوى غير موجود
            if (levelExists == null)
            {
                return NotFound(new { message = "Level name does not exist." });
            }


            // جلب الفيدباك إذا كانت البيانات موجودة
            var feedbacks = await _context.student_Teacher_Feedbacks
                .Include(f => f.Student)
                .ThenInclude(s => s.User)
                .Include(f => f.Teacher)
                .ThenInclude(t => t.User)
                .Where(f => _context.student_classes
                                    .Any(sc => sc.Student_ID == f.Student_ID
                                               && sc.classs.Class_Name == className
                                               && sc.classs.Stage == stage
                                               && sc.classs.Level == level) &&
                            _context.teacher_Classes
                                    .Any(tc => tc.Teacher_ID == f.Teacher_ID
                                               && tc.Class.Class_Name == className
                                               && tc.Class.Stage == stage
                                               && tc.Class.Level == level))
                .Select(f => new StudentTeacherFeedbackDto
                {
                    Student_Teacher_Feedback_Id = f.Student_Teacher_Feedback_Id,
                    StudentName = f.Student.User.Full_Name,
                    TeacherName = f.Teacher.User.Full_Name,
                    Feedback = f.Feedback,
                    Date = f.date
                })
                .ToListAsync();

            if (feedbacks == null || feedbacks.Count == 0)
            {
                return NotFound(new { message = "No feedbacks found for the specified class, stage, and level." });
            }

            return Ok(feedbacks);
        }






        #region delete
        [HttpDelete("{id}")]
        public async Task<IActionResult>delete(int id ,string messeage)
        {
            var feed = await _context.student_Teacher_Feedbacks.Include(f=>f.Student.User).
                FirstOrDefaultAsync(f => f.Student_Teacher_Feedback_Id == id);

          if(feed!= null)

          {
                emailFeedback.SendDeleteEmail(feed.Student.User.Email,feed.Student.User.Full_Name,messeage);

                _context.student_Teacher_Feedbacks.Remove(feed);
                _context.SaveChanges();
                return Ok();
          }
          return NoContent();

        }
        #endregion



    }
    
} 

