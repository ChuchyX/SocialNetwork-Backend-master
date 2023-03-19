using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;


namespace SocialNetwork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(DataContext context, IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _environment = env;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto request)
        {
            var user = await _userService.Register(request);
            if (user == null) return BadRequest("There is already a registered user with that email");
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseLogin>> Login(UserLoginDto request)
        {
            var result = await _userService.Login(request);
            if (result == null)
            {
                return BadRequest("User Not Found. Wrong Username or Password");
            }
            else
            {
                var response = result.Item1;
                string nameImg = result.Item2;
                if (nameImg != "")
                {
                    var filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);

                    using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fs.CopyToAsync(ms);
                            response.User.image = File(ms.ToArray(), "image/jpg", fileDownloadName: nameImg + ".jpg");
                            fs.Close();
                        }
                    }
                }
                else
                    response.User.image = null;

                return Ok(response);
            }
        }

        [HttpGet("getme")]
        public async Task<ActionResult<Tuple<ReturnUser,string>>> GetMe()
        {
            var result = await _userService.GetMe();
            if (result == null) return BadRequest("User Not Found!");
            var user = result.Item1;
            var nameImg = result.Item2;        
            if (nameImg != "")
            {
                var filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);

                using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (var ms = new MemoryStream())
                    {
                        await fs.CopyToAsync(ms);
                        user.image = File(ms.ToArray(), "image/jpg", fileDownloadName: nameImg + ".jpg");
                        fs.Close();
                    }
                }
            }
            else
                user.image = null;
            return Ok(user);
        }

        [HttpPost("uploadProfilePicture")]
        public async Task<ActionResult<ReturnUser>> UploadPP(IFormFile file)
        {
            var result = await _userService.UploadPP(file);
            var user = result.Item1;
            string nameImg = result.Item2;
            var filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);
            using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var ms = new MemoryStream())
                {
                    await fs.CopyToAsync(ms);
                    user.image = File(ms.ToArray(), "image/jpg", fileDownloadName: nameImg + ".jpg");
                    fs.Close();
                }
            }
            return Ok(user);
        }    
    }
}
