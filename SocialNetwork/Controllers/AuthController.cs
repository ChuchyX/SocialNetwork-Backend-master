using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
        private readonly DataContext _context;

        public AuthController(DataContext context, IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _environment = env;
            _context = context;
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
        public async Task<ActionResult<ReturnUser>> GetMe()
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


        //Refactorizar despues: hacer metodo mediante servicio user
        [HttpPost("addpost")]
        public async Task<ActionResult<List<Post>>> AddPost([FromForm] PostDto postdto)
        {        
            var email = _userService.GetMyEmail();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            
            int cantPostUser = _context.Posts.Count(post => post.UserId == user.Id);

            string nameImg = "p-" + user.Id.ToString() + "-" + cantPostUser + 1;                      
            var filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);
            var fs = new FileStream(filename, FileMode.Create);
            await postdto.picture.CopyToAsync(fs);
            fs.Close();


            Post post = new Post();
            post.Content = postdto.content;
            post.UserId = user.Id;
            post.Picture = filename;
            user.Posts.Add(post);
            await _context.SaveChangesAsync();


            //A partir de aqui crear un postResponse, para enviar una lista de postResponse al front

            ReturnUser returnUser = new ReturnUser();
            returnUser.email = "correcto@gmail.com";

            return Ok(returnUser);
        }

    }
}
