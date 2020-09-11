using DotNetCoreIdentity.Helpers;
using DotNetCoreIdentity.Models;
using DotNetCoreIdentity.Models.Identity;
using DotNetCoreIdentity.Models.RequestModels;
using DotNetCoreIdentity.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetCoreIdentity.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationSettings _applicationSettings;

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _applicationSettings = appSettings.Value;
        }

        // POST api/Authentication/register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        return await Task.FromResult<IActionResult>(Ok(new { message = "UserName Already Registered." }));
                    }

                    var applicationUser = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Name = model.Name,
                        LastName = model.LastName,
                        Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(applicationUser, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(applicationUser, model.Role);
                        return await Task.FromResult<IActionResult>(Created("api/authentication/register", result));
                    }
                    return await Task.FromResult<IActionResult>(Ok(string.Join(",", result.Errors?.Select(error => error.Description))));
                }
                catch (Exception)
                {
                    return await Task.FromResult<IActionResult>(Ok(new { message = "Something went wrong." }));
                }
            }

            string errorMessage = string.Join(", ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
            return await Task.FromResult<IActionResult>(BadRequest(errorMessage ?? "Bad Request"));
        }

        // POST api/Authentication/login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    ApplicationUser appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.UserName);
                    TokenResponse tokenResponse = AuthenticationHelper.GenerateJwtToken(appUser, _configuration);

                    return await Task.FromResult<IActionResult>(Ok(tokenResponse));
                }
                return await Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.Unauthorized, "Bad Credentials"));
            }
            string errorMessage = string.Join(", ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
            return await Task.FromResult<IActionResult>(BadRequest(errorMessage ?? "Bad Request"));
        }
    }
}