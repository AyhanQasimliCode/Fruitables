using Microsoft.AspNetCore.Identity;

namespace Fruitables.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
