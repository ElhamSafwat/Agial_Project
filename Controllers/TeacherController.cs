using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using final_project_Api.DTOs;
using Microsoft.AspNetCore.Identity;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly AgialContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeacherController
               (AgialContext context,
                RoleManager<IdentityRole> roleManager,
                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Url http://localhost:5175/api/Teacher   // GetAllTeacherWithSubject
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherWithSubjectDTO>>> GetTeachers()
        {
            var teachersWithSubjects = await _context.teachers
                .Include(t => t.User)   // To access the ApplicationUser details
                .Include(t => t.subject) // To access the Subject details
                .Where(t => !t.IsDelete) // Only include non-deleted teachers
                .Select(t => new TeacherWithSubjectDTO
                {
                    TeacherId = t.User.Id,   // Get the User Id
                    TeacherName = t.User.Full_Name, // From ApplicationUser
                    phoneNumber = t.User.PhoneNumber,
                    SubjectName = t.subject.Subject_Name, // From Subject
                    HireDate = t.HireDate, // From Teacher
                    IsDelete = t.IsDelete
                })
                .ToListAsync();

            return Ok(teachersWithSubjects);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Url http://localhost:5175/api/Teacher/{userId}     // GetTeacherWithID
        [HttpGet("{userId}")]
        public async Task<ActionResult<TeacherWithSubjectDTO>> GetTeacherByUserId(string userId)
        {
            // Find the teacher based on the provided UserId
            var teacherWithSubject = await _context.teachers
                .Include(t => t.User)   // To access the ApplicationUser details
                .Include(t => t.subject) // To access the Subject details
                .Where(t => t.UserId == userId && !t.IsDelete) // Filter by UserId and non-deleted teachers
                .Select(t => new TeacherWithSubjectDTO
                {
                    TeacherId = t.User.Id,   // Get the User Id
                    TeacherName = t.User.Full_Name, // From ApplicationUser
                    SubjectName = t.subject.Subject_Name, // From Subject
                    HireDate = t.HireDate // From Teacher
                })
                .FirstOrDefaultAsync();

            // If teacher is not found, return 404
            if (teacherWithSubject == null)
            {
                return NotFound($"Teacher with UserId '{userId}' not found.");
            }

            return Ok(teacherWithSubject);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("AddTeacher")]     // AddTeacher
        public async Task<ActionResult> AddTeacher(AddTeacherDTO addTeacherDTO)
        {
            // Check if password and confirm password match
            if (addTeacherDTO.Password != addTeacherDTO.ConfirmPassword)
            {
                return BadRequest("كلمه السر غير مطابقه فى تاكيد كلمه السر");
            }
            var existingUser = await _userManager.FindByEmailAsync(addTeacherDTO.Email);
            if (existingUser != null)
            {
                return BadRequest("البريد الإلكتروني موجود بالفعل");
            }
            var user = new ApplicationUser
            {
                UserName = addTeacherDTO.UserName,
                Full_Name = addTeacherDTO.Full_Name,
                Email = addTeacherDTO.Email,
                EmailConfirmed = true,
                PhoneNumber = addTeacherDTO.Phone
            };

            var result = await _userManager.CreateAsync(user, addTeacherDTO.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // If user creation was successful, create a Teacher record
            var teacher = new Teacher
            {
                UserId = user.Id,
                HireDate = addTeacherDTO.HireDate,
                IsDelete = false,
                Subject_ID = addTeacherDTO.Subject_ID
            };
            // If user creation was successful, create a Teacher_Stage record
            var teacher_satage = new Teacher_Stage
            {
                Teacher_Id = user.Id,
                Stage = addTeacherDTO.Stage,
            };

            _context.teachers.Add(teacher);
            _context.teacher_Stages.Add(teacher_satage);
            // Assign the "Teacher" role to the new user
            var roleExists = await _roleManager.RoleExistsAsync("Teacher");
            if (roleExists)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");
                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }
            }
            else
            {
                return BadRequest("The 'Teacher' role does not exist.");
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully  with the 'Teacher' role." });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////

        // Url http://localhost:5175/api/Teacher/DeleteTeacher/{userId}
        [HttpDelete("DeleteTeacher/{userId}")]  // DeleteTeacherInsertIntoIsDeleteJustTrueNotDelete
        public async Task<ActionResult> DeleteTeacher(string userId)
        {
            var teacher = await _context.teachers
                .Include(t => t.User) 
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDelete);

            if (teacher == null)
            {
                return NotFound($"Teacher with UserId '{userId}' not found.");
            }
            teacher.IsDelete = true;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Teacher deleted successfully." });
        }
    }
}
