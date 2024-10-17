using Microsoft.AspNetCore.Identity;

namespace ProgPoePart2_6212.Models
{
    public class User : IdentityUser
    {
          public string FullName { get; set; }

}
}
