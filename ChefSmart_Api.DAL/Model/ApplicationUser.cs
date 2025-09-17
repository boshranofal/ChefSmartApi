using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.DAL.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? CodeResetPassword { get; set; }
        public DateTime? CodeResetExpiration { get; set; }
    }
}
