using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentExamForTeacherController : ControllerBase
    {
        private readonly AgialContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public StudentExamForTeacherController(AgialContext context,
                              UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("InsertExamResult")]
        public async Task<IActionResult> InsertExamResult(StudentExamForTeacher studentExamDto)
        {
            // تأكيد الطلاب موجودين
            var student = await _context.students.FindAsync(studentExamDto.Student_Id);
            if (student == null)
            {
                return NotFound("هذا الطالب غير موجود.");
            }

            // تأكيد الامتحان موجود
            var exam = await _context.exam.FindAsync(studentExamDto.Exam_Id);
            if (exam == null)
            {
                return NotFound("هذا الامتحان غير موجود");
            }

            // صلاحيه الامتحان مع هذا الطالب
            if (exam.Teacher_ID != studentExamDto.Teacher_Id)
            {
                return BadRequest("هذا المعلم ليس لديه الصلاحية لتصحيح هذا الامتحان.");
            }

            if (studentExamDto.Degree < 0)
            {
                return BadRequest("لا يمكن أن تكون الدرجة أقل من 0.");
            }

            if (studentExamDto.Degree > exam.Max_Degree)
            {
                return BadRequest($"{exam.Max_Degree} اقصى درجه لهذا الامتحان هى");
            }

            // لو الطالب خد الامتحان قبل كده
            var existingStudentExam = _context.student_Exams
                .FirstOrDefault(se => se.Student_ID == studentExamDto.Student_Id && se.Exam_ID == studentExamDto.Exam_Id);

            if (existingStudentExam != null)
            {
                return BadRequest("هذا الطالب قد أجرى هذا الامتحان مسبقًا.");
            }

            // تأكيد المدرس مع الطلاب في نفس الكلاس
            var teacherClass = await _context.teacher_Classes
                .FirstOrDefaultAsync(tc => tc.Teacher_ID == studentExamDto.Teacher_Id);

            if (teacherClass == null)
            {
                return BadRequest("لم يتم العثور على الفصل الخاص بالمعلم.");
            }

            var studentClass = await _context.student_classes
                .FirstOrDefaultAsync(sc => sc.Student_ID == studentExamDto.Student_Id && sc.Class_ID == teacherClass.Class_ID);

            if (studentClass == null)
            {
                return BadRequest("الطالب والمعلم ليسوا في نفس الفصل.");
            }

            // Create a new Student_Exam record
            var newStudentExam = new Student_Exam
            {
                Student_ID = studentExamDto.Student_Id,
                Exam_ID = studentExamDto.Exam_Id,
                Degree = studentExamDto.Degree
            };

            // Add the new record to the database
            _context.student_Exams.Add(newStudentExam);
            await _context.SaveChangesAsync();

            // Check if the student's degree is below the MinDegree (Fail) or above (Pass)
            if (studentExamDto.Degree < exam.Min_Degree)
            {
                return Ok("تم إدخال نتيجة الامتحان وهذا الطالب ساقط في هذا الامتحان.");
            }
            else
            {
                return Ok("تم إدخال نتيجة الامتحان وهذا الطالب ناجح في هذا الامتحان.");
            }
        }

        /* ***************************************************************************** */
        [HttpPut("UpdateStudentDegree")]
        public async Task<IActionResult> UpdateStudentDegree(StudentExamForTeacher studentExamDto)
        {
            // Check if the student exists
            var student = await _context.students.FindAsync(studentExamDto.Student_Id);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Check if the exam exists
            var exam = await _context.exam.FindAsync(studentExamDto.Exam_Id);
            if (exam == null)
            {
                return NotFound("Exam not found.");
            }

            // Check if the student has already taken this exam
            var studentExam = _context.student_Exams
                .FirstOrDefault(se => se.Student_ID == studentExamDto.Student_Id && se.Exam_ID == studentExamDto.Exam_Id);

            if (studentExam == null)
            {
                return NotFound("This student has not taken the exam yet.");
            }

            // Check if the new degree is greater than the exam's Max Degree
            if (studentExamDto.Degree > exam.Max_Degree)
            {
                return BadRequest($"The degree cannot exceed the maximum allowed degree of {exam.Max_Degree}.");
            }

            // Update the degree
            studentExam.Degree = studentExamDto.Degree;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok("Student exam degree updated successfully.");
        }

        /* ***************************************************************************** */
        [HttpGet("GetAllStudentExams")]
        public async Task<IActionResult> GetAllStudentExams()
        {
            var studentExams = await _context.student_Exams
                .Include(se => se.Student)        // Student data
                .ThenInclude(s => s.User)         // ApplicationUser for Student's Name
                .Include(se => se.Exam)           // Exam data
                .ThenInclude(e => e.Tech)         // the Teacher data in the Exam
                .ThenInclude(t => t.User)         // ApplicationUser for Teacher's Name
                .Select(se => new StudentExamResultForTeacherDto
                {
                    StudentName = se.Student.User.Full_Name,  
                    TeacherName = se.Exam.Tech.User.Full_Name, 
                    Exam_Id = se.Exam.Exam_ID,
                    Degree = se.Degree,
                    SubjectName = se.Exam.subject_name,  // Map Subject_Name
                    ClassName = se.Exam.class_name
                })
                .ToListAsync();

            return Ok(studentExams);
        }

        /* ***************************************************************************** */

        [HttpGet("GetStudentExamsByExamId/{examId}")]
        public async Task<IActionResult> GetStudentExamsByExamId(int examId)
        {
            // Check if the exam exists
            var exam = await _context.exam.FindAsync(examId);
            if (exam == null)
            {
                return NotFound("هذا اامتحان ليس موجود");
            }

            // Retrieve student exams for the given exam ID
            var studentExams = await _context.student_Exams
                .Where(se => se.Exam_ID == examId) // Filter by Exam ID
                .Include(se => se.Student)        // Include Student data
                .ThenInclude(s => s.User)         // Include the associated ApplicationUser for Student's Name
                .Include(se => se.Exam)           // Include Exam data
                .ThenInclude(e => e.Tech)         // Include the Teacher data in the Exam
                .ThenInclude(t => t.User)         // Include the associated ApplicationUser for Teacher's Name
                .Select(se => new StudentExamResultForTeacherDto
                {
                    StudentName = se.Student.User.Full_Name,  // Assuming the User table has FullName
                    TeacherName = se.Exam.Tech.User.Full_Name, // Assuming the Teacher's User table has FullName
                    Exam_Id = se.Exam.Exam_ID,  // Customize this if you have a name for the exam
                    Degree = se.Degree,
                    SubjectName = se.Exam.subject_name,  // Map Subject_Name
                    ClassName = se.Exam.class_name
                })
                .ToListAsync();
            if (!studentExams.Any())
            {
                return NotFound("هذا الامتحان لم يمتحنوا طلاب بعد"); // This student has not taken any exams
            }
            return Ok(studentExams);
        }

        /* ***************************************************************************** */

        [HttpGet("GetStudentExamsByStudentId/{studentId}")]
        public async Task<IActionResult> GetStudentExamsByStudentId(string studentId)
        {
            // Check if the student exists
            var student = await _context.students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("هذا الطالب ليس موجود.");
            }

            // Retrieve exams for the given Student ID
            var studentExams = await _context.student_Exams
                .Where(se => se.Student_ID == studentId) // Filter by Student ID
                .Include(se => se.Student)        
                .ThenInclude(s => s.User)         
                .Include(se => se.Exam)           
                .ThenInclude(e => e.Tech)        
                .ThenInclude(t => t.User)         
                .Select(se => new StudentExamResultForTeacherDto
                {
                    StudentName = se.Student.User.Full_Name,  
                    TeacherName = se.Exam.Tech.User.Full_Name, 
                    Exam_Id =  se.Exam.Exam_ID,  
                    Degree = se.Degree,
                    SubjectName = se.Exam.subject_name,  // Map Subject_Name
                    ClassName = se.Exam.class_name
                })
                .ToListAsync();
            if (!studentExams.Any())
            {
                return NotFound("هذا الطالب لم يمتحن بعد");
            }
            return Ok(studentExams);
        }
    }
}
