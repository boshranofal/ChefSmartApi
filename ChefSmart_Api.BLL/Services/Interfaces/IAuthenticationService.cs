using ChefSmart_Api.DAL.DTO.Request;
using ChefSmart_Api.DAL.DTO.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserResponse> Login(LoginRequest request);
        Task<UserResponse> Register(RegisterRequest request,HttpRequest httpRequest);
        Task<string> ConfirmEmail(string userId, string token);
        Task<string> ForgetPassword(ForgetPasswordRequest request);
        Task<string> VerifyResetCode(VerifyResetCodeRequest request);
        Task<string> ResetPassword(ResetPasswordRequest request);
    }
}
