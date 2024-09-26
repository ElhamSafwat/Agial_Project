using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPIDotNet.DTO;
using System.Threading.Tasks;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly AgialContext dbContext; 

        public AccountController(UserManager<ApplicationUser> UserManager, IConfiguration config, AgialContext dbContext)
        {
            userManager = UserManager;
            this.config = config;
            this.dbContext = dbContext;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto UserFromRequest)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Full_Name = UserFromRequest.FullName,
                    UserName = UserFromRequest.UserName,
                    Email = UserFromRequest.Email,
                    PhoneNumber = UserFromRequest.Phone_Number
                };

                IdentityResult result = await userManager.CreateAsync(user, UserFromRequest.Password);
                if (result.Succeeded)
                {
                   
                    if (!string.IsNullOrEmpty(UserFromRequest.Role))
                    {
                        await userManager.AddToRoleAsync(user, UserFromRequest.Role);
                    }

                    
                    if (UserFromRequest.Role == "Parent")
                    {
                        var parent = new Parent
                        {
                            UserId = user.Id
                        };
                        dbContext.parent.Add(parent);
                    }
                    else if (UserFromRequest.Role == "Student")
                    {
                        var student = new Student
                        {
                            UserId = user.Id,
                            enrollmentDate = UserFromRequest.enrollmentDate ,
                            Parent_ID=UserFromRequest.Parent_ID
                        };
                        dbContext.students.Add(student);
                    }
                    else if (UserFromRequest.Role == "Teacher")
                    {
                        var teacher = new Teacher
                        {
                            UserId = user.Id,
                            HireDate = DateTime.UtcNow ,
                            IsDelete=UserFromRequest.IsDelete,
                            Subject_ID=UserFromRequest.Subject_ID
                        };
                        dbContext.teachers.Add(teacher);
                    }

                    await dbContext.SaveChangesAsync(); 
                    return Ok("User created successfully.");
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }
            }

            return BadRequest(ModelState);
        }
    }
}
