using final_project_Api.Admin_ClassDTO;
using final_project_Api.DTO;
using final_project_Api.Models;
using final_project_Api.Parentdtos;
using final_project_Api.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AgialContext context;
        private readonly IEmailService emailService;
        private readonly UserManager<ApplicationUser> userManager;

        public StudentController(AgialContext _context,UserManager<ApplicationUser> _userManager, IEmailService _emailService)
        {
            this.context = _context;
            this.userManager = _userManager;
            this.emailService = _emailService;
        }

        #region Get All Students
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult getStudents()
        {
            List<Student> students= context.students.Include(s => s.User).Include(s => s.parent.User).ToList();
            List<GetStudentDTO> studentDTOs = new List<GetStudentDTO>();
            foreach (Student stud in students)
            {
                GetStudentDTO studDTO = new GetStudentDTO();
                studDTO.Student_Id=stud.UserId;
                studDTO.fullName = stud.User.Full_Name;
                studDTO.Student_Email = stud.User.Email;
                studDTO.Phone_Number = stud.User.PhoneNumber;
                studDTO.enrollmentDate = stud.enrollmentDate;
                studDTO.Stage = stud.Stage;
                studDTO.Level = stud.Level;
                studDTO.Parent_Name = stud.parent.User.Full_Name;
                studentDTOs.Add(studDTO);
            }
            return Ok(studentDTOs);
        }
        #endregion

        #region Get Student By ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult getStudentById(string id)
        {
            var student=context.students.Include(s => s.User).Include(s=>s.parent.User).FirstOrDefault(s=>s.UserId==id);
            if (student == null)
            {
                return NotFound(new {message="لا يوجد طالب "});
            }
            GetStudentDTO studentDTO = new GetStudentDTO
            {
                Student_Id=student.UserId,
                fullName = student.User.Full_Name,
                Student_Email=student.User.Email,
                Phone_Number=student.User.PhoneNumber,
                enrollmentDate=student.enrollmentDate,
                Stage=student.Stage,
                Level=student.Level,
                Parent_Name=student.parent.User.Full_Name
            };
            return Ok(studentDTO);
        }
        #endregion
        
        #region Get Student By Name (UserName)
        [HttpGet("{name:alpha}")]
        public IActionResult getStudentByName(string name)
        {
            var student = context.students.Include(s => s.User).Include(s => s.parent.User).FirstOrDefault(s => s.User.UserName==name);
            if (student == null)
            {
                return NotFound(new {message="لا يوجد طالب بهذا الاسم "});
            }
            GetStudentDTO studentDTO = new GetStudentDTO
            {
                Student_Id = student.UserId,
                fullName = student.User.Full_Name,
                Student_Email = student.User.Email,
                Phone_Number = student.User.PhoneNumber,
                enrollmentDate = student.enrollmentDate,
                Stage = student.Stage,
                Level = student.Level,
                Parent_Name = student.parent.User.Full_Name
            };
            return Ok(studentDTO);
        }
        #endregion

        #region Create New Student
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddStudent(AddStudentDTo AddStudDto)
        {
            var existingUser = await userManager.FindByEmailAsync(AddStudDto.Student_Email);
            if (existingUser != null)
            {
                return Ok(new { message = "A student with this email already exists." });
            }
            if (await userManager.FindByNameAsync(AddStudDto.Student_Name) != null)
            {
                return Ok (new { message = $"{AddStudDto.fullName}اسم مستخدم مستخدم من قبل من فضلك اختار اسم مستخدم مختلف لي طالب ." });
            }
            if (AddStudDto.Password != AddStudDto.ConfirmPassword)
            {
                return Ok(new {message= $"كلمه المرور وكلمه تاكيد لحساب طالب غير متطابقين  {AddStudDto.Student_Name}." });
            }

            var user = new ApplicationUser
            {
                UserName = AddStudDto.Student_Name,
                Full_Name = AddStudDto.fullName,
                Email =AddStudDto.Student_Email,
                EmailConfirmed = true,
                PhoneNumber =AddStudDto.Phone_Number
            };
            var result = await userManager.CreateAsync(user, AddStudDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            // تعيين دور للطالب
            await userManager.AddToRoleAsync(user, "Student");
            // تحقق من Stage و Level
            if (AddStudDto.Stage == "أبتدائي")
            {
                if (AddStudDto.Level < 1 || AddStudDto.Level > 6)
                {
                    return BadRequest(new { message = "Level must be between 1 and 6 for أبتدائي stage." });
                }
            }
            else if (AddStudDto.Stage == "أعدادي" || AddStudDto.Stage == "ثانوي")
            {
                if (AddStudDto.Level < 1 || AddStudDto.Level > 3)
                {
                    return BadRequest(new { message = $"Level must be between 1 and 3 for {AddStudDto.Stage} stage." });
                }
            }
            else
            {
                return BadRequest(new { message = $"Invalid stage '{AddStudDto.Stage}'. Must be 'أبتدائي', 'أعدادي', or 'ثانوي'." });
            }
            var student = new Student
            {
                UserId = user.Id,
                enrollmentDate=AddStudDto.enrollmentDate,
                Stage = AddStudDto.Stage,
                Level = AddStudDto.Level,
                Parent_ID =AddStudDto.Parent_ID
            };
            context.students.Add(student);
           
            await context.SaveChangesAsync();
            // Send email
            await emailService.SendRegistrationEmail(AddStudDto.Student_Email, AddStudDto.Student_Name, AddStudDto.Password);


            return Ok(new { message = "Student created successfully.." });
        }
        #endregion

        #region Delate Student
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DelaeteStudent(string id)
        {
            var student = context.students.Include(s => s.User)
                                                   .Include(s => s.parent)
                                                   .Include(s => s.Student_Classes)
                                                   .Include(s => s.Payments)
                                                   .Include(s => s.Students_Teachers_FeedBack)
                                                   .Include(s => s.Session_Students)
                                                   .Include(s => s.Student_Exams)
                                                   .Include(s=>s.Parent_Teacher_Feedbacks)
                                                   .FirstOrDefault(s => s.UserId == id);
            if (student == null)
            {
                return NotFound(new {message="لا يوجد هذا الطالب "});
            }
            try
            {
               
                if(student.Student_Classes != null)
                {
                    context.student_classes.RemoveRange(student.Student_Classes);
                }
                if (student.Parent_Teacher_Feedbacks != null)
                {
                    context.parent_Teacher_Feedbacks.RemoveRange(student.Parent_Teacher_Feedbacks);
                }
                if (student.Payments != null)
                {
                    context.payments.RemoveRange(student.Payments);
                }
                if(student.Students_Teachers_FeedBack != null)
                {
                    context.student_Teacher_Feedbacks.RemoveRange(student.Students_Teachers_FeedBack);
                }
                if(student.Session_Students != null)
                {
                    context.Session_Students.RemoveRange(student.Session_Students);
                }
                if(student.Student_Exams != null)
                {
                    context.student_Exams.RemoveRange(student.Student_Exams);
                }
               
                context.students.Remove(student);

                var studentRoles = await context.UserRoles.Where(r=>r.UserId==student.UserId).ToListAsync();
               
                if (studentRoles.Any())
                {
                    context.UserRoles.RemoveRange(studentRoles);
                }

                // 12: Delete the students' from AspNetUsers
                var studentUsers = await context.Users.Where(s => s.Id == id).ToListAsync();

                if (studentUsers.Any())
                {
                    context.Users.RemoveRange(studentUsers);
                }

                await context.SaveChangesAsync();
                return Ok(new { message = "تم حذف الطالب بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the student: " + ex.Message);
            }
           
        }
        #endregion

        #region Edit Student
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStudent(string id, Edit_StudentDTO studentDTO)
        {
            try
            {
                var student =await context.students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == id);
                if (student == null)
                {
                    return NotFound(new { message = "هذا الطالب غير موجود من فضلك ادخل الرقم الصحيح" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                // تحقق من Stage و Level
                if (studentDTO.Stage == "أبتدائي")
                {
                    if (studentDTO.Level < 1 || studentDTO.Level > 7)
                    {
                        return BadRequest(new { message = "المستوي يجب ان يكون من 1 الي 6 الي المرحله الابتدائيه" });
                    }
                }
                else if (studentDTO.Stage == "أعدادي" || studentDTO.Stage == "ثانوي")
                {
                    if (studentDTO.Level < 1 || studentDTO.Level > 4)
                    {
                        return BadRequest(new { message = $"المستوي يجب ان يكون بين 1 و 3 للمرحله {studentDTO.Stage}." });
                    }
                }
                else
                {
                    return BadRequest(new { message = $"غير صحيح  '{studentDTO.Stage}'. يجب ان تكون أبتدائي', 'أعدادي' او 'ثانوي'" });
                }

                student.User.Full_Name = studentDTO.Student_Name;
                student.User.Email = studentDTO.Student_Email;
                student.User.PhoneNumber = studentDTO.Phone_Number;
                student.Stage = studentDTO.Stage;
                student.Level = studentDTO.Level;

                context.SaveChanges();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
            return NoContent();
        }

        #endregion


        #region get all classes for one teacher
        [HttpGet("{teacherId}/teachers")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetClassByTeacher(string teacherId)
        {
            var teachers = await context.teacher_Classes
                .Where(tc => tc.Teacher_ID == teacherId)
                .Select(tc => new GetTeachersToClassDTO
                {
                    classid = tc.Class.Class_ID,
                    className=tc.Class.Class_Name
                })
                .ToListAsync();

            return Ok(teachers);

        }
        #endregion

        #region get students for one class

        [HttpGet("{classId}/students")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var teachers = await context.student_classes
                .Where(tc => tc.Class_ID == classId)
                .Select(tc => new GetTeachersToClassDTO
                {
                    UserID = tc.Student_ID,
                    FullName = tc.students.User.Full_Name

                })
                .ToListAsync();

            return Ok(teachers);

        }
        #endregion

        #region get degree for assingment =>one student
        [HttpGet("assignmentgrades/{studentId}")]
        [Authorize(Roles = "Parent,Student")]
        public async Task<ActionResult<IEnumerable<getAssignmentForStudentDTO>>> GetGradesByStudentId(string studentId)
        {
            var grades = await context.Session_Students
                .Include(ss => ss.Session)
                .Where(ss => ss.Student_ID == studentId && ss.Degree != 0) // تأكد من أن الدرجة ليست null
                .Select(ss => new getAssignmentForStudentDTO
                {
                    Session_ID = ss.Session_ID,
                    studentId = ss.Student_ID,
                    Date = DateOnly.FromDateTime(ss.Session.Date),
                    Assignment = ss.Assignment,
                    Degree = ss.Degree,
                    subject_Name = ss.Session.Material_Name
                })
                .ToListAsync();

            if (grades == null || !grades.Any())
            {
                return NotFound(new { message = "No grades found for the specified student's assignments." });
            }

            return Ok(grades);
        }


        #endregion

        #region get all assingment for one student
        [HttpGet("unsubmittedassignments/{studentId}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<getAssignmentForStudentDTO>>> GetUnsubmittedAssignmentsByStudentId(string studentId)
        {
            var assignments = await context.Session_Students
                .Include(ss => ss.Session)
                .Where(ss => ss.Student_ID == studentId && ss.Degree == 0 && ss.Assignment!=null) // فقط المهام التي لم تُسلم
                .Select(ss => new getAssignmentForStudentDTO
                {
                    Session_ID = ss.Session_ID,
                    studentId = ss.Student_ID,
                    Date = DateOnly.FromDateTime(ss.Session.Date),
                    Assignment = ss.Assignment,
                    subject_Name = ss.Session.Material_Name
                })
                .ToListAsync();

            if (assignments == null || !assignments.Any())
            {
                return NotFound(new { message = "No unsubmitted assignments found for the specified student." });
            }

            return Ok(assignments);
        }

        #endregion+



        #region get all teachers for one student
        [HttpGet("teachers/{studentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTeachersByStudent(string studentId)
        {
            var teachers = await context.student_classes
                .Where(sc => sc.Student_ID == studentId) // الحصول على الفصول الخاصة بالطالب
                .SelectMany(sc => context.teacher_Classes
                    .Where(tc => tc.Class_ID == sc.Class_ID) // البحث عن المدرسين في هذه الفصول
                    .Select(tc => new GetTeachersOneStudent
                    {
                        UserID = tc.Teacher_ID,
                        className = tc.Class.Class_Name,
                        FullName = tc.Teacher.User.Full_Name, // افترض أن لديك خاصية لاسم المدرس

                    }))
                .Distinct() // للتأكد من عدم وجود تكرارات
                .ToListAsync();

            return Ok(teachers);
        }
        #endregion
    }
}
