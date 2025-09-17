using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.DAL.DTO.Request
{
    public class VerifyResetCodeRequest
    {
        public string Email { get; set; }
        public string CodeResetPassword { get; set; }
    }
}
