using ChefSmart_Api.BLL.Services.Interfaces;
using ChefSmart_Api.DAL.DTO.Request;
using ChefSmart_Api.DAL.DTO.Response;
using ChefSmart_Api.DAL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.BLL.Services.Classess
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthenticationService(UserManager<ApplicationUser> userManager,
            IConfiguration configuration, IEmailSender emailSender)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }
        public async Task<UserResponse> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود. يرجى التسجيل أولاً.");
            }

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new Exception("يرجى تأكيد بريدك الإلكتروني قبل تسجيل الدخول.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                throw new Exception("كلمة المرور غير صحيحة. يرجى المحاولة مرة أخرى.");
            }
            return new UserResponse
            {
                Token = await CreateToken(user)
            };
        }

        public async Task<UserResponse> Register(RegisterRequest request,HttpRequest httpRequest)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var escapedToken = Uri.EscapeDataString(token);
                var emailUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/api/Account/confirm-email?token={escapedToken}&userId={user.Id}";
                await _emailSender.SendEmailAsync(user.Email, "تأكيد البريد الإلكتروني",
                    $"<h1>مرحباً {user.UserName}</h1>" +
                    $"<p>يرجى تأكيد بريدك الإلكتروني من خلال النقر على الرابط أدناه:</p>" +
                    $"<a href='{emailUrl}'>تأكيد البريد الإلكتروني</a>");
                return new UserResponse
                {
                    Token = "تم التسجيل بنجاح. يرجى فحص بريدك الإلكتروني لتأكيد حسابك."
                };
            }
            else
            {
                
                if (result.Errors.Any(e => e.Code == "DuplicateEmail"))
                {
                    throw new Exception("البريد الإلكتروني الذي أدخلته مستخدم بالفعل. يرجى استخدام بريد إلكتروني آخر.");
                }
                
                throw new Exception("فشل التسجيل: هذا البريد الإلكتروني غير متاح.");
            }
        }

        public async Task<string> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود.");
            }

            // URL decode the token if needed
            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                return "تم تأكيد البريد الإلكتروني بنجاح. يمكنك الآن تسجيل الدخول.";
            }
            return "فشل تأكيد البريد الإلكتروني: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        private async Task<string> CreateToken(ApplicationUser user)
        {
            var Claims = new List<Claim>
            {
                new Claim("UserName",user.UserName),
                new Claim("Email",user.Email),
                new Claim("Id",user.Id)
            };
            var Roles = await _userManager.GetRolesAsync(user);
            foreach (var role in Roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWTOptions")["SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                 claims: Claims,
                expires: DateTime.Now.AddDays(5),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<string> ForgetPassword(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new Exception("المستخدم غير موجود.");
            }
            var random = new Random();
            var code = random.Next(10000, 99999).ToString();
            user.CodeResetPassword = code;
            user.CodeResetExpiration = DateTime.Now.AddMinutes(10);
            await _userManager.UpdateAsync(user);
            await _emailSender.SendEmailAsync(user.Email, "إعادة تعيين كلمة المرور",
              $"<h1>إعادة تعيين كلمة المرور</h1>" +
              $"<p>رمز إعادة التعيين الخاص بك هو: {code}</p>");

            return "تم إرسال رمز إعادة التعيين إلى بريدك الإلكتروني.";
        }

        public async Task<string> VerifyResetCode(VerifyResetCodeRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new Exception("البريد الإلكتروني غير موجود");
            }
            if (user.CodeResetPassword != request.CodeResetPassword || user.CodeResetExpiration < DateTime.Now)
            {
                throw new Exception("رمز غير صحيح أو منتهي الصلاحية.");
            }
            return "تم التحقق من الرمز بنجاح.";
        }

        public async Task<string> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new Exception("البريد الإلكتروني غير موجود");
            }
            if (user.CodeResetPassword != request.CodeResetPassword || user.CodeResetExpiration < DateTime.Now)
            {
                throw new Exception("رمز غير صحيح أو منتهي الصلاحية.");
            }

            // Validate password match before processing
            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new Exception("كلمات المرور غير متطابقة.");
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("فشل في إزالة كلمة المرور القديمة.");
            }
            result = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception("فشل في تعيين كلمة المرور الجديدة: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            user.CodeResetPassword = null;
            user.CodeResetExpiration = null;
            await _userManager.UpdateAsync(user);
            return "تم إعادة تعيين كلمة المرور بنجاح.";

        }
    }
}
