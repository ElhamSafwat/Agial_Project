using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPIDotNet.DTO;
using System.Threading.Tasks;
using final_project_Api.DTO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using final_project_Api.Parentdtos;

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




        [HttpPost("Login")]

        public async Task<IActionResult> Login(LoginDTO userlogin)
        {
            if (!ModelState.IsValid)
            {
               return BadRequest(new { message = "من فضلك ادخل داتا مطلوبه " });
            }

            // check user name or email vaild
            ApplicationUser user = await userManager.FindByNameAsync(userlogin.email_username)
                                  ?? await userManager.FindByEmailAsync(userlogin.email_username);

            if (user == null)
            {
                return BadRequest(new { message = "اسم مستخدم او اميل غير صحيح " });
            }

            // check passwored 
            bool isPasswordValid = await userManager.CheckPasswordAsync(user, userlogin.password);
            if (!isPasswordValid)
            {
                
                return BadRequest(new {message="من فضلك ادخل كلمه مرور صحيحه "});
            }

           //to add some date in token
            var userClaims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName)
    };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecritKey"]));
            var signingCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                audience: config["JWT:AudienceIP"],
                issuer: config["JWT:IssuerIP"],
                expires: DateTime.Now.AddDays(50),
                claims: userClaims,
                signingCredentials: signingCredentials
            );

            // return token to store in client site
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                expiration = DateTime.Now.AddDays(50) 
            });
        }


        #region get_profile
        [HttpGet("{id}")]
        public async Task<IActionResult> get(string id)
        {
            ApplicationUser user=await userManager.FindByIdAsync(id);
            if (user == null) 
            {
                return BadRequest();
            }
            profileDto pro = new profileDto();

            pro.full_name = user.Full_Name;
            pro.phone = user.PhoneNumber;
            pro.email = user.Email;
            pro.username = user.UserName;

            return Ok(pro);
        }
        #endregion

        #region edit profile

        [HttpPut("{id}")]
        public async Task<IActionResult> edit(string id,ProfileEditeDTO proedit)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(new {message="من فضلك ادخل داتا صحيجه"});
            }

            ApplicationUser user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                user.Full_Name = proedit.full_name;
                user.PhoneNumber = proedit.phone;
                
               
                if (user.Email == proedit.email)
                {
                    user.Email = proedit.email;
                }

                else if (await userManager.FindByEmailAsync(proedit.email) != null)
                {
                    return BadRequest(new{message="تلك ايميل موجود بالفعل" });
                }
                else
                {
                    user.Email = proedit.email;
                }

                //if(user.UserName== proedit.username)
                //{
                //    user.UserName = proedit.username;
                //}
                //else if(await userManager.FindByNameAsync(proedit.username)!=null)
                //{
                //    return BadRequest(new { message = "اسم مستخدم مستخدم بالفعل " });
                //}
                //else
                //{
                //    user.UserName = proedit.username;

                //}
                 dbContext.Users.Update(user);
                dbContext.SaveChanges();
                

                return Ok(new { message = "تم تعديل بنجاح" });
            }

            return BadRequest();
            

            
        }

        #endregion



        #region edit password
        [HttpPut("password/{id}")]
        public async Task<IActionResult> edit_password(string id, passwordEdit proedit)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "من فضلك ادخل داتا صحيجه" });
            }

            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {


                if (!await userManager.CheckPasswordAsync(user, proedit.oldpassword))
                {
                    return BadRequest(new { message = "من فضلك ادخل كلمه المرور القديمه صحيحه" });
                }
            

                
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
                return Ok(new { message = "تم تعديل بنجاح" });
            }

            return BadRequest();



        }
        #endregion
    }
}
