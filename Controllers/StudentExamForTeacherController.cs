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
        [HttpPut("UpdateStudentDegree/{studentId}/{examId}/{teacherId}")]
        public async Task<IActionResult> UpdateStudentDegree(string studentId, int examId, string teacherId, [FromBody] float degree)
        {
            // التحقق مما إذا كان الطالب موجودًا
            var student = await _context.students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // التحقق مما إذا كان الامتحان موجودًا
            var exam = await _context.exam.FindAsync(examId);
            if (exam == null)
            {
                return NotFound("Exam not found.");
            }

            // التحقق مما إذا كان المدرس موجودًا
            var teacher = await _context.teachers.FindAsync(teacherId); // Assuming you have a teachers table
            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            // التحقق مما إذا كان الطالب قد أجرى هذا الامتحان بالفعل
            var studentExam = _context.student_Exams
                .FirstOrDefault(se => se.Student_ID == studentId && se.Exam_ID == examId);

            if (studentExam == null)
            {
                return NotFound("This student has not taken the exam yet.");
            }

            // التحقق مما إذا كانت الدرجة الجديدة تتجاوز الدرجة القصوى للامتحان
            if (degree > exam.Max_Degree)
            {
                return BadRequest($"The degree cannot exceed the maximum allowed degree of {exam.Max_Degree}.");
            }

            // تحديث الدرجة
            studentExam.Degree = degree;

            // حفظ التعديلات في قاعدة البيانات
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
            // تحقق من وجود الامتحان
            var exam = await _context.exam.FindAsync(examId);
            if (exam == null)
            {
                return NotFound("هذا الامتحان ليس موجود");
            }

            var studentExams = await _context.student_Exams
                .Where(se => se.Exam_ID == examId)
                .Include(se => se.Student)
                .ThenInclude(s => s.User)   // للحصول على بيانات الطالب
                .Include(se => se.Student)
                .ThenInclude(s => s.parent) // الربط مع جدول الوالد
                .Include(se => se.Exam)
                .ThenInclude(e => e.Tech)
                .ThenInclude(t => t.User)
                .Select(se => new
                {
                    StudentID = se.Student_ID,
                    StudentName = se.Student.User.Full_Name,
                    ExamID = se.Exam.Exam_ID,
                    Degree = se.Degree,
                    MaxDegree = se.Exam.Max_Degree,
                    MinDegree = se.Exam.Min_Degree,
                    SubjectName = se.Exam.subject_name,
                    Status = se.Degree < se.Exam.Min_Degree ? "سيء" : "جيد",

                    // Fetch the parent's phone number from AspNetUsers using Parent's UserId
                    ParentPhone = _context.Users
                        .Where(u => u.Id == se.Student.parent.UserId)
                        .Select(u => u.PhoneNumber)
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (!studentExams.Any())
            {
                return NotFound("هذا الامتحان لم يمتحنوا طلاب بعد");
            }

            return Ok(studentExams);
        }
        /* ***************************************************************************** */
        // new E.Eman
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
                .Select(se => new getStudentExamDegreeDTO
                {
                    StudentName = se.Student.User.Full_Name,
                    TeacherName = se.Exam.Tech.User.Full_Name,
                    Exam_Id = se.Exam.Exam_ID,
                    Exam_Date = DateOnly.FromDateTime(se.Exam.Exam_Date),
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
        /* ***************************************************************************** */

        [HttpGet("GetStudentDegree/{studentId}/{examId}")]
        public async Task<IActionResult> GetStudentDegree(string studentId, int examId)
        {
            // Check if the student exists
            var student = await _context.students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("هذا الطالب ليس موجود.");
            }

            // Check if the exam exists
            var exam = await _context.exam.FindAsync(examId);
            if (exam == null)
            {
                return NotFound("هذا الامتحان ليس موجود.");
            }

            // Retrieve the student's exam result and additional information
            var studentExam = await _context.student_Exams
                .Where(se => se.Student_ID == studentId && se.Exam_ID == examId)
                .Select(se => new
                {
                    StudentName = se.Student.User.Full_Name,
                    Degree = se.Degree,
                    MaxDegree = exam.Max_Degree,
                    MinDegree = exam.Min_Degree
                })
                .FirstOrDefaultAsync();

            if (studentExam == null)
            {
                return NotFound("هذا الطالب لم يمتحن في هذا الامتحان.");
            }

            // Return student name, degree, max degree, and min degree
            return Ok(studentExam);
        }


        #region newWithFare
        [HttpGet("students/{teacherId}")]
        public async Task<ActionResult<List<ClassWithStudentsDTO>>> GetStudentsByTeacherId(string teacherId)
        {
            // Retrieve the teacher and their associated classes
            var teacher = await _context.teachers
                .Include(t => t.teacher_Classes)
                .ThenInclude(tc => tc.Class)
                .FirstOrDefaultAsync(t => t.UserId == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            var classWithStudentsList = new List<ClassWithStudentsDTO>();

            foreach (var teacherClass in teacher.teacher_Classes)
            {
                // Get the class name
                var className = teacherClass.Class.Class_Name;

                // Get students in the class
                var studentsInClass = await _context.students
                    .Where(s => s.Student_Classes.Any(sc => sc.Class_ID == teacherClass.Class_ID))
                    .Select(s => new StudentDTO
                    {
                        StudentName = s.User.Full_Name, // Assign student name
                        StudentId = s.UserId // Assign student ID
                    }).ToListAsync();

                // Create the result object for the current class
                var result = new ClassWithStudentsDTO
                {
                    ClassName = className, // Assign the class name
                    Students = studentsInClass // Assign the list of students
                };

                classWithStudentsList.Add(result);
            }

            return Ok(classWithStudentsList); // Return the list of class results
        }
        #endregion

        #region newWithFaresToReturnWithTeacherIDAndDateReturnTheExam
        [HttpGet("/Teacher/{Teacher_id}/date/{date}")]
        public async Task<IActionResult> Get_Exam_By_Teacher_And_Date(string Teacher_id, DateTime date)
        {
            try
            {
                // Convert date to DateOnly for comparison
                var datefrom = DateOnly.FromDateTime(date);

                // Fetch exams for the given teacher and date
                List<Exam> exams = await _context.exam
                    .Include(e => e.Tech)
                    .Include(e => e.Tech.User)
                    .Where(e => e.Teacher_ID == Teacher_id && DateOnly.FromDateTime(e.Exam_Date) == datefrom)
                    .ToListAsync();

                if (exams.Count == 0) // Check if no exams were found
                {
                    return NotFound($"No exams found for Teacher '{Teacher_id}' on '{date.ToShortDateString()}'.");
                }

                // Create a list of DTOs
                List<Get_ExamByTeacherAndDateDTO> examDTOs = new List<Get_ExamByTeacherAndDateDTO>();

                // Map exam data to DTOs
                foreach (var exam in exams)
                {
                    Get_ExamByTeacherAndDateDTO examDTO = new Get_ExamByTeacherAndDateDTO
                    {
                        Exam_ID = exam.Exam_ID,
                        Exam_Date = exam.Exam_Date,
                        Start_Time = exam.Start_Time,
                        End_Time = exam.End_Time,
                        Min_Degree = exam.Min_Degree,
                        Max_Degree = exam.Max_Degree,
                        class_name = exam.class_name,
                        subject_name = exam.subject_name,
                        Teacher_Name = exam.Tech.User.Full_Name
                    };
                    examDTOs.Add(examDTO);
                }

                return Ok(examDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

    }
}
