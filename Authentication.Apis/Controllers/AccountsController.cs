using Authentication.Apis.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Authentication.Apis.Controllers
{
    [Route("accounts/[action]")]
    [ApiController]
    public class AccountsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountsController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterAccountInputModel registrationInput)
        {
            var user = await _userManager.FindByNameAsync(registrationInput.UserName);
            if (user != null)
            {
                return BadRequest("User already exists.");
            }

            user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registrationInput.UserName,
                Email = registrationInput.Email
            };

            var result = await _userManager.CreateAsync(user, registrationInput.Password);
            if (!result.Succeeded)
            {
                var resultErrors = result.Errors.Select(e => e.Description);
                return BadRequest(string.Join("\n", resultErrors));
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginAccountInputModel loginInput)
        {
            var user = await _userManager.FindByNameAsync(loginInput.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginInput.Password))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Ok("Login successful");
            }

            return BadRequest("Invalid Username or Password");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logout Successful.");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return Ok(await _userManager.FindByIdAsync(loggedInUserId));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(DeleteAccountInputModel input)
        {
            var loggedInUserName = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.FindByNameAsync(loggedInUserName);

            if (user == null)
            {
                return BadRequest("User doesn't exist.");
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, input.Password);
            if (!isValidPassword)
            {
                return BadRequest("Incorrect Password.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to delete user account. Please try again.");
            }

            return Ok("Account deleted successfully.");
        }
    }
}
