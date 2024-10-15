using final_project_Api.DTO;
using final_project_Api.Models;
using final_project_Api.Parentdtos;
using final_project_Api.Serviece;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly AgialContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService emailService;

        public ParentController(AgialContext context,
                                UserManager<ApplicationUser> userManager,
                                RoleManager<IdentityRole> roleManager, IEmailService _emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            this.emailService=_emailService;
        }

        #region create parent and student
        // POST: api/Parent/AddParent
        [HttpPost("AddParent")]
        public async Task<ActionResult> AddParent(ParentDTO parentDTO)
        {
            var errors = new List<string>();
           
            // تحقق من كلمات المرور
            if (parentDTO.Password != parentDTO.ConfirmPassword)
            {
                errors.Add("كلمه المرور وتاكيد كلمه مرور غير متطابقين.");
            }

            // تحقق من وجود البريد الإلكتروني للأهل
            if (await _userManager.FindByEmailAsync(parentDTO.Email) != null)
            {
                errors.Add("حساب ولي الامر موجود بالفعل .");
            }
            if(await _userManager.FindByNameAsync(parentDTO.UserName)!=null)
            {
                errors.Add("اسم مستخدم مستخدم من قبل من فضلك اختار اسم مستخدم مختلف لي ولي امر .");
            }
            // تحقق من بيانات الطلاب
            foreach (var studentDTO in parentDTO.Students)
            {
                // تحقق من وجود البريد الإلكتروني طلاب
                if (await _userManager.FindByEmailAsync(studentDTO.Student_Email) != null)
                {
                    errors.Add($"حساب طالب  موجود بالفعل  من فضلك اختار حساب تاني طالب {studentDTO.fullName} .");
                }
                if (await _userManager.FindByNameAsync(studentDTO.Student_Name) != null)
                {
                    errors.Add($"{studentDTO.fullName}اسم مستخدم مستخدم من قبل من فضلك اختار اسم مستخدم مختلف لي طالب .");
                }
                if (studentDTO.Password != studentDTO.ConfirmPassword)
                {
                    errors.Add($"كلمه المرور وكلمه تاكيد لحساب طالب غير متطابقين  {studentDTO.Student_Name}.");
                }

                if (studentDTO.Stage == "أبتدائي")
                {
                    if (studentDTO.Level < 1 || studentDTO.Level > 6)
                    {
                        errors.Add($"Level for student {studentDTO.Student_Name} must be between 1 and 6 for أبتدائي stage.");
                    }
                }
                else if (studentDTO.Stage == "أعدادي" || studentDTO.Stage == "ثانوي")
                {
                    if (studentDTO.Level < 1 || studentDTO.Level > 3)
                    {
                        errors.Add($"Level for student {studentDTO.Student_Name} must be between 1 and 3 for {studentDTO.Stage} stage.");
                    }
                }
                //else
                //{
                //    errors.Add($"Invalid stage '{studentDTO.Stage}' for student {studentDTO.Student_Name}.");
                //}
            }

            // إذا كان هناك أخطاء، قم بإرجاعها
            if (errors.Count > 0)
            {
                return BadRequest(new { Message = "There were validation errors.", Errors = errors });
            }

            // إنشاء مستخدم الأهل
            var parentUser = new ApplicationUser
            {
                UserName = parentDTO.UserName,
                Full_Name = parentDTO.FullName,
                Email = parentDTO.Email,
                PhoneNumber = parentDTO.Phone,
                EmailConfirmed = true
            };

            var createParentResult = await _userManager.CreateAsync(parentUser, parentDTO.Password);
            if (!createParentResult.Succeeded)
            {
                return BadRequest(createParentResult.Errors);
            }

            // إضافة سجل الأهل
            var parent = new Parent { UserId = parentUser.Id };
            _context.parent.Add(parent);

            // تعيين دور للأهل
            await _userManager.AddToRoleAsync(parentUser, "Parent");

            await emailService.SendRegistrationEmail(parentDTO.Email, parentDTO.UserName, parentDTO.Password);


            // إضافة الطلاب
            foreach (var studentDTO in parentDTO.Students)
            {
                var studentUser = new ApplicationUser
                {
                    UserName = studentDTO.Student_Name,
                    Full_Name = studentDTO.fullName,
                    Email = studentDTO.Student_Email,
                    PhoneNumber = studentDTO.Phone_Number,
                    EmailConfirmed = true
                };

                var createStudentResult = await _userManager.CreateAsync(studentUser, studentDTO.Password);
                if (!createStudentResult.Succeeded)
                {
                    return BadRequest(createStudentResult.Errors);
                }

                // إضافة سجل الطالب
                var student = new Student
                {
                    UserId = studentUser.Id,
                    enrollmentDate = studentDTO.enrollmentDate,
                    Stage = studentDTO.Stage,
                    Level = studentDTO.Level,
                    Parent_ID = parent.UserId
                };
                _context.students.Add(student);
                // تعيين دور للطالب
                await _userManager.AddToRoleAsync(studentUser, "Student");
                await emailService.SendRegistrationEmail(studentDTO.Student_Email, studentDTO.Student_Name, studentDTO.Password);

            }


            //check to sure this parent have student if not delete it
            var have_student=await _context.parent.Include(parent => parent.Students).FirstOrDefaultAsync(p=>p.UserId== parent.UserId);
            if (have_student != null)
            {
                if (have_student.Students == null)
                {
                    _context.parent.Remove(have_student);
                    await _context.SaveChangesAsync();
                    return BadRequest(new { message = "لن تسطيع اضافه بيانات ولي لامر من فضلك ادخل بيانات ابنائه صحيحه " });

                }

            }
            
            // حفظ جميع التغييرات
            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم اضافه حساب ولي الامر وابنائه بنجاح" });
        }
        #endregion

        #region all function parent 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetParentDTO>>> GetParents()
        {
            var Parentehitstudent = await _context.parent
                .Include(t => t.User)       // To access the ApplicationUser details
                .Include(t => t.Students)   // To include related students
                .ThenInclude(s => s.User)   // To access Student's User details
                .Select(t => new GetParentDTO
                {
                    UserId = t.User.Id,                     // Get the User Id
                    FullName = t.User.Full_Name,            // Parent's Full Name
                    Fullphone = t.User.PhoneNumber,         // Parent's Phone Number
                    Email = t.User.Email,                   // Parent's Email
                    Studentname = t.Students
                                    .Select(s => s.User.Full_Name)  // List of Student names
                                    .ToList()
                })
                .ToListAsync();

            return Ok(Parentehitstudent);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<GetParentDTO>> GetParentById(string id)
        {
            var parent = await _context.parent
                .Include(p => p.User)
                .Include(p => p.Students)
                .Where(p => p.User.Id == id)
                .Select(p => new GetParentDTO
                {
                    UserId = p.User.Id,
                    FullName = p.User.Full_Name,
                    Fullphone = p.User.PhoneNumber,
                    Email = p.User.Email,
                    Studentname = p.Students.Select(s => s.User.Full_Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (parent == null)
            {
                return NotFound(new {message="لا يوجد ولي امر"}); // Return 404 if the parent is not found
            }

            return Ok(parent);
        }
        [HttpGet("byname/{name}")]
        public async Task<ActionResult<IEnumerable<GetParentDTO>>> GetParentByName(string name)
        {
            var parents = await _context.parent
                .Include(p => p.User)
                .Include(p => p.Students)
                .Where(p => p.User.Full_Name.Contains(name)) // Use Contains for partial match
                .Select(p => new GetParentDTO
                {
                    UserId = p.User.Id,
                    FullName = p.User.Full_Name,
                    Email = p.User.Email,
                    Fullphone = p.User.PhoneNumber,
                    Studentname = p.Students.Select(s => s.User.Full_Name).ToList()
                })
                .ToListAsync();

            if (parents == null || !parents.Any())
            {
                return NotFound("لا توجد اولياء امور بتلك اسماء"); // Return 404 if no parents are found
            }

            return Ok(parents);
        }

        //// PUT: api/Parent/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParent(string id, ParentUpdateDTO parentUpdateDTO)
        {
            var parent = await _context.parent
                .Include(p => p.User) // Eagerly load the User entity
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (parent == null)
            {
                return NotFound(new { message = $"Parent userid {id} not found." });
            }

            if (parent.User == null)
            {
                return NotFound(new { message = $"User for parent with userid {id} not found." });
            }

            // Update the student's list for the parent
            parent.User.Full_Name = parentUpdateDTO.Full_Name;
            parent.User.PhoneNumber = parentUpdateDTO.phoneNumber;
            if (parent.User.Email == parentUpdateDTO.Email)
            {
                parent.User.Email = parentUpdateDTO.Email;

            }
           
            else if (await _userManager.FindByEmailAsync(parentUpdateDTO.Email) != null)
            {
                    return BadRequest(new { message = "تلك اميل موجود بالفعل " });
            }
            else
            {
                parent.User.Email = parentUpdateDTO.Email;
            }
            
            // Update the user in the database
            var result = await _userManager.UpdateAsync(parent.User);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تحديث الحساب بنجاح " });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParent(string id)
        {
            // 1: Find the parent and their students
            var parent = await _context.parent
                .Include(p => p.Students)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (parent == null)
            {
                return NotFound(new { message = $"Parent with id {id} not found." });
            }

            // 2: Delete student exams from Student_Exam table
            var studentIds = parent.Students.Select(s => s.UserId).ToList();
            var studentExams = await _context.student_Exams
                .Where(se => studentIds.Contains(se.Student_ID))
                .ToListAsync();

            if (studentExams.Any())
            {
                _context.student_Exams.RemoveRange(studentExams);
            }

            // 3: Delete feedback  from Student_Teacher_Feedback table 
            var studentFeedbacks = await _context.student_Teacher_Feedbacks
                .Where(stf => studentIds.Contains(stf.Student_ID))
                .ToListAsync();

            if (studentFeedbacks.Any())
            {
                _context.student_Teacher_Feedbacks.RemoveRange(studentFeedbacks);
            }

            // 4: Delete  session  from Session_Student 
            var sessionStudents = await _context.Session_Students
                .Where(ss => studentIds.Contains(ss.Student_ID))
                .ToListAsync();

            if (sessionStudents.Any())
            {
                _context.Session_Students.RemoveRange(sessionStudents);
            }

            // 5: Delete the payments  with the students
            var studentPayments = await _context.payments
                .Where(p => studentIds.Contains(p.Student_ID))
                .ToListAsync();

            if (studentPayments.Any())
            {
                _context.payments.RemoveRange(studentPayments);
            }
            // delete from parent_teacher feedback
            var studentparentteacher_feed=await _context.parent_Teacher_Feedbacks.Where(p=> studentIds.Contains(p.Student_ID)).ToListAsync();
            if (studentparentteacher_feed.Count>0)
            {
                _context.parent_Teacher_Feedbacks.RemoveRange(studentparentteacher_feed);
            }
            //delete from student_class
            var student_class=await _context.student_classes.Where(p => studentIds.Contains(p.Student_ID))
                .ToListAsync();
            if (student_class.Count > 0) 
            { 
              _context.student_classes.RemoveRange(student_class);
            }
            // 6: Delete the students  with this parent from the Students table
            if (parent.Students.Any())
            {
                _context.students.RemoveRange(parent.Students);
            }

            // 7: Delete parent's roles from AspNetUserRoles table
            var parentRoles = await _context.UserRoles
                .Where(ur => ur.UserId == parent.User.Id)
                .ToListAsync();

            if (parentRoles.Any())
            {
                _context.UserRoles.RemoveRange(parentRoles);
            }

            // 8: Delete parent from AspNetUsers table
            _context.Users.Remove(parent.User);

            // 9: Delete the parent from the Parents table
            _context.parent.Remove(parent);


            await _context.SaveChangesAsync();

            // 11: Delete the students roles from AspNetUserRoles table
            var studentRoles = await _context.UserRoles
                .Where(ur => studentIds.Contains(ur.UserId))
                .ToListAsync();

            if (studentRoles.Any())
            {
                _context.UserRoles.RemoveRange(studentRoles);
            }

            // 12: Delete the students' from AspNetUsers
            var studentUsers = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .ToListAsync();

            if (studentUsers.Any())
            {
                _context.Users.RemoveRange(studentUsers);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم المسح  بنجاح" });  // Return his Message
        }


        #endregion

        [HttpGet("liststudent/{parentId}")]
        public async Task<ActionResult<List<GetstudentparentbyidDTO>>> GetstudentbyParentById(string parentId)
        {
            var students = await _context.parent
                .Include(p => p.Students)
                .Where(p => p.User.Id == parentId)
                .SelectMany(p => p.Students)
                .Select(s => new GetstudentparentbyidDTO
                {
                    StudentId = s.User.Id,
                    StudentName = s.User.Full_Name
                })
                .ToListAsync();

            if (students == null || students.Count == 0)
            {
                return NotFound(new {message="لا بوجد ابناء لولي الاهر هذا "}); // Return 404 if no students are found
            }

            return Ok(students);
        }



    }
}
