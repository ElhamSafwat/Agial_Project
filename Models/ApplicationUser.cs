using Microsoft.AspNetCore.Identity;

namespace final_project_Api.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string Full_Name { get; set; }
    }
}
