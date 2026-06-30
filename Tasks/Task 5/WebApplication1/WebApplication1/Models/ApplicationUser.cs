using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public List<Booking> Bookings { get; set; } = new();
    }
}
