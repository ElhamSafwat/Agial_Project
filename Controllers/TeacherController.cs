using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using final_project_Api.DTOs;
using Microsoft.AspNetCore.Identity;
using final_project_Api.Serviece;
using Microsoft.AspNetCore.Authorization;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly AgialContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService emailService;
        public TeacherController
               (AgialContext context,
                RoleManager<IdentityRole> roleManager,
                UserManager<ApplicationUser> userManager, IEmailService _emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            emailService=_emailService;
        }

        // Url http://localhost:5175/api/Teacher   // GetAllTeacherWithSubject
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TeacherWithSubjectDTO>>> GetTeachers()
        {
            var teachersWithSubjects = await _context.teachers
                .Include(t => t.User)   // To access the ApplicationUser details
                .Include(t => t.subject) // To access the Subject details
                .Include(t => t.teacher_Stages)
                .Where(t => !t.IsDelete) // Only include non-deleted teachers
                .Select(t => new TeacherWithSubjectDTO
                {
                    TeacherId = t.User.Id,   // Get the User Id
                    TeacherName = t.User.Full_Name, // From ApplicationUser
                    phoneNumber = t.User.PhoneNumber,
                    SubjectName = t.subject.Subject_Name, // From Subject
                    HireDate = t.HireDate, // From Teacher
                    stage = string.Join(", ", t.teacher_Stages.Select(ts => ts.Stage)),
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
                .Include(t => t.teacher_Stages)
                .Select(t => new TeacherWithSubjectDTO
                {
                    TeacherId = t.User.Id,   // Get the User Id
                    TeacherName = t.User.Full_Name, // From ApplicationUser
                    phoneNumber = t.User.PhoneNumber,
                    SubjectName = t.subject.Subject_Name, // From Subject
                    HireDate = t.HireDate, // From Teacher
                    stage = string.Join(", ", t.teacher_Stages.Select(ts => ts.Stage))

                })
                .FirstOrDefaultAsync();

            // If teacher is not found, return 404
            if (teacherWithSubject == null)
            {
                return NotFound(new { message = $"Teacher with UserId '{userId}' not found." });
            }

            return Ok(teacherWithSubject);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("AddTeacher")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> AddTeacher(AddTeacherDTO addTeacherDTO)
        {
            // Check if password and confirm password match
            if (addTeacherDTO.Password != addTeacherDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "كلمه السر غير مطابقه فى تاكيد كلمه السر" });
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(addTeacherDTO.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "البريد الإلكتروني موجود بالفعل" });
            }

            // Check if the Subject_ID exists in the Subjects table
            var subjectExists = await _context.subjects.AnyAsync(s => s.Subject_ID == addTeacherDTO.Subject_ID);
            if (!subjectExists)
            {
                return BadRequest(new { message = "هذه الماده غير موجوده" });
            }

            // Start a transaction to ensure that all operations either succeed or fail together
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create the ApplicationUser
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
                    // If user creation failed, rollback and return errors
                    await transaction.RollbackAsync();
                    return BadRequest(result.Errors);
                }

                // Create the Teacher record
                var teacher = new Teacher
                {
                    UserId = user.Id,
                    HireDate = addTeacherDTO.HireDate,
                    IsDelete = false,
                    Subject_ID = addTeacherDTO.Subject_ID
                };

                // Create the Teacher_Stage record
                var teacher_stage = new Teacher_Stage
                {
                    Teacher_Id = user.Id,
                    Stage = addTeacherDTO.Stage
                };

                _context.teachers.Add(teacher);
                _context.teacher_Stages.Add(teacher_stage);
                emailService.SendRegistrationEmail(addTeacherDTO.Email, addTeacherDTO.UserName, addTeacherDTO.Password);


                // Assign the "Teacher" role to the new user
                var roleExists = await _roleManager.RoleExistsAsync("Teacher");
                if (roleExists)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");
                    if (!roleResult.Succeeded)
                    {
                        // If role assignment failed, rollback and return errors
                        await transaction.RollbackAsync();
                        return BadRequest(roleResult.Errors);
                    }
                }
                else
                {
                    // If the role doesn't exist, rollback the transaction
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "The 'Teacher' role does not exist." });
                }

                // Save changes to the database and commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "User registered successfully with the 'Teacher' role." });
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of any error
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        // Url http://localhost:5175/api/Teacher/DeleteTeacher/{userId}
        [HttpDelete("DeleteTeacher/{userId}")]  // DeleteTeacherInsertIntoIsDeleteJustTrueNotDelete
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTeacher(string userId)
        {
            var teacher = await _context.teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDelete);

            if (teacher == null)
            {
                return NotFound(new { message = $"Teacher with UserId '{userId}' not found." });
            }

            // Get the user (teacher) from the UserManager
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = $"User with UserId '{userId}' not found in AspNetUsers." });
            }

            // Get the user's roles
            var roles = await _userManager.GetRolesAsync(user);

            // Remove the teacher's roles (if any)
            if (roles != null && roles.Count > 0)
            {
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, roles);
                if (!removeRoleResult.Succeeded)
                {
                    return BadRequest(new { message = "Failed to remove roles from the teacher." });
                }
            }

            // Mark the teacher as deleted (set IsDelete to true)
            teacher.IsDelete = true;


            //// Set the Subject_ID to null
            //teacher.Subject_ID = null;

            // Save changes to the database
             _context.SaveChanges();

            return Ok(new { Message = "Teacher deleted successfully, including from AspNetUserRoles." });
        }

    }
}
