using final_project_Api.DTO;
using final_project_Api.Models;
using final_project_Api.Parentdtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AgialContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public StudentController(AgialContext _context,UserManager<ApplicationUser> _userManager)
        {
            this.context = _context;
            this.userManager = _userManager;
        }

        #region Get All Students
        [HttpGet]
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
        public IActionResult getStudentById(string id)
        {
            var student=context.students.Include(s => s.User).Include(s=>s.parent.User).FirstOrDefault(s=>s.UserId==id);
            if (student == null)
            {
                return NotFound();
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
                return NotFound();
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
        public async Task<ActionResult> AddStudent(AddStudentDTo AddStudDto)
        {
            var existingUser = await userManager.FindByEmailAsync(AddStudDto.Student_Email);
            if (existingUser != null)
            {
                return BadRequest("A student with this email already exists.");
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
            // تحقق من Stage و Level
            if (AddStudDto.Stage == "أبتدائي")
            {
                if (AddStudDto.Level < 1 || AddStudDto.Level > 6)
                {
                    return BadRequest("Level must be between 1 and 6 for أبتدائي stage.");
                }
            }
            else if (AddStudDto.Stage == "أعدادي" || AddStudDto.Stage == "ثانوي")
            {
                if (AddStudDto.Level < 1 || AddStudDto.Level > 3)
                {
                    return BadRequest($"Level must be between 1 and 3 for {AddStudDto.Stage} stage.");
                }
            }
            else
            {
                return BadRequest($"Invalid stage '{AddStudDto.Stage}'. Must be 'أبتدائي', 'أعدادي', or 'ثانوي'.");
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
            return Ok("Student created successfully..");
        }
        #endregion

        #region Delate Student
        [HttpDelete("{id}")]
        public async Task<IActionResult> DelaeteStudent(string id)
        {
            var student = context.students.Include(s => s.User)
                                                   .Include(s => s.parent)
                                                   .Include(s => s.Student_Classes)
                                                   .Include(s => s.Payments)
                                                   .Include(s => s.Students_Teachers_FeedBack)
                                                   .Include(s => s.Session_Students)
                                                   .Include(s => s.Student_Exams)
                                                   .FirstOrDefault(s => s.UserId == id);
            if (student == null)
            {
                return NotFound();
            }
            try
            {
                if (student.parent != null)
                {
                    context.parent.RemoveRange(student.parent);
                }
                if(student.Student_Classes != null)
                {
                    context.student_classes.RemoveRange(student.Student_Classes);
                }
                if(student.Payments != null)
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

                var user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result= await userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest("Error deleting user from Identity.");
                    }
                }
                await context.SaveChangesAsync();
                return Ok("تم حذف الطالب بنجاح");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the student: " + ex.Message);
            }
           
        }
        #endregion

        #region Edit Student
        [HttpPut("{id}")]
        public async Task<IActionResult> EditStudent(string id, Edit_StudentDTO studentDTO)
        {
            try
            {
                var student =await context.students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == id);
                if (student == null)
                {
                    return NotFound("هذا الطالب غير موجود من فضلك ادخل الرقم الصحيح");
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
                        return BadRequest("المستوي يجب ان يكون من 1 الي 6 الي المرحله الابتدائيه");
                    }
                }
                else if (studentDTO.Stage == "أعدادي" || studentDTO.Stage == "ثانوي")
                {
                    if (studentDTO.Level < 1 || studentDTO.Level > 4)
                    {
                        return BadRequest($"المستوي يجب ان يكون بين 1 و 3 للمرحله {studentDTO.Stage}.");
                    }
                }
                else
                {
                    return BadRequest($"غير صحيح  '{studentDTO.Stage}'. يجب ان تكون أبتدائي', 'أعدادي' او 'ثانوي'");
                }

                student.User.Full_Name = studentDTO.Student_Name;
                student.User.Email = studentDTO.Student_Email;
                student.User.PhoneNumber = studentDTO.Phone_Number;
                student.Stage = studentDTO.Stage;
                student.Level = studentDTO.Level;

                if (!string.IsNullOrEmpty(studentDTO.New_Password))
                {
                    var passwordResult = await userManager.ChangePasswordAsync(student.User, studentDTO.Old_Password, studentDTO.New_Password);

                    if (!passwordResult.Succeeded)
                    {
                        return BadRequest(passwordResult.Errors);
                    }
                }
            
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while updating the student.");
            }
            return NoContent();
        }

        #endregion

    }
}
