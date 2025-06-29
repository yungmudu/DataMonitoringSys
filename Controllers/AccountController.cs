using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DataMonitoringSys.Models;
using System.ComponentModel.DataAnnotations;

namespace DataMonitoringSys.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("/Account/LoginPost")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("/Account/Login?error=Invalid input");
            }

            var result = await _signInManager.PasswordSignInAsync(
                request.Email, 
                request.Password, 
                request.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            else if (result.IsLockedOut)
            {
                return Redirect("/Account/Login?error=Account is locked out");
            }
            else if (result.IsNotAllowed)
            {
                return Redirect("/Account/Login?error=Login not allowed. Email may need confirmation");
            }
            else if (result.RequiresTwoFactor)
            {
                return Redirect("/Account/Login?error=Two-factor authentication required");
            }
            else
            {
                return Redirect("/Account/Login?error=Invalid email or password");
            }
        }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
