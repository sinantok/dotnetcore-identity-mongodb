using DotNetCoreIdentity.Models.Identity;
using DotNetCoreIdentity.Models.RequestModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
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

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Register([FromBody] UserRegister model)
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
    }
}