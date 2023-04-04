using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public async Task<ActionResult<List<PostResponse>>> AddPost([FromForm] PostDto postdto)
        {        
            var email = _userService.GetMyEmail();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            
            int cantPostUser = _context.Posts.Count(post => post.UserId == user.Id);

            var filename = "";
            string nameImg = "";
            if (postdto.picture != null)
            {
                nameImg = "p-" + user.Id.ToString() + "-" + cantPostUser + 1;
                filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);
                var fs = new FileStream(filename, FileMode.Create);
                await postdto.picture.CopyToAsync(fs);
                fs.Close();
            }
            


            Post post = new Post();
            post.Content = postdto.content;
            post.UserId = user.Id;
            post.Picture = filename;
            user.Posts.Add(post);
            await _context.SaveChangesAsync();

            List<PostResponse> postResponses = new List<PostResponse>();
            var allposts = await _context.Posts.ToListAsync();
            foreach (var p in allposts)
            {
                PostResponse currpResponse = new PostResponse();
                currpResponse.Id = p.Id;
                currpResponse.Content = p.Content;
                currpResponse.Date = p.Date;
                currpResponse.Likes = p.Likes;
                currpResponse.Comentarios = p.Comentarios.ToList();
                filename = p.Picture;
                if (filename != "")
                {
                    using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fs.CopyToAsync(ms);
                            currpResponse.Image = File(ms.ToArray(), "image/jpg", fileDownloadName: nameImg + ".jpg");
                            fs.Close();
                        }
                    }
                }
                else
                    currpResponse.Image = null;
               
                var userContext = _context.Users.FirstOrDefault(x => x.Id == p.UserId);
                ReturnUser returnUser = new ReturnUser();
                returnUser.id = userContext.Id;
                returnUser.username = userContext.Username;
                returnUser.email = userContext.Email;
                returnUser.edad = userContext.Edad;
                returnUser.sexo = userContext.Sexo;
                filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", userContext.ProfilePicture);
                if (userContext.ProfilePicture != "")
                {
                    using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fs.CopyToAsync(ms);
                            returnUser.image = File(ms.ToArray(), "image/jpg", fileDownloadName: nameImg + ".jpg");
                            fs.Close();
                        }
                    }
                }
                else
                    returnUser.image = null;
               
                currpResponse.User = returnUser;
                postResponses.Add(currpResponse);
            }         

            return Ok(postResponses);
        }

        [HttpGet("allposts")]
        public async Task<ActionResult<List<PostResponse>>> AllPosts()
        {
            List<PostResponse> postResponses = new List<PostResponse>();
            var allposts = await _context.Posts.ToListAsync();
            foreach (var p in allposts)
            {
                PostResponse currpResponse = new PostResponse();
                currpResponse.Id = p.Id;
                currpResponse.Content = p.Content;
                currpResponse.Date = p.Date;
                currpResponse.Likes = p.Likes;
                currpResponse.Since = _userService.TiempoTranscurrido(p.Date);
                currpResponse.Comentarios = _context.Comentarios.Where(c => c.PostId == p.Id).ToList();
                currpResponse.Comentarios.Reverse();

                for (int i = 0; i < currpResponse.Comentarios.Count; i++)
                {
                    currpResponse.Comentarios[i].Since = _userService.TiempoTranscurrido(currpResponse.Comentarios[i].Date);
                }
                var filename = p.Picture;              
                if (filename != "")
                {
                    using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fs.CopyToAsync(ms);
                            currpResponse.Image = File(ms.ToArray(), "image/jpg", fileDownloadName: p.Id + ".jpg");
                            fs.Close();
                        }
                    }
                }
                else
                    currpResponse.Image = null;

                var userContext = _context.Users.FirstOrDefault(x => x.Id == p.UserId);
                ReturnUser returnUser = new ReturnUser();
                returnUser.id = userContext.Id;
                returnUser.username = userContext.Username;
                returnUser.email = userContext.Email;
                returnUser.edad = userContext.Edad;
                returnUser.sexo = userContext.Sexo;
                filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", userContext.ProfilePicture);
                if (userContext.ProfilePicture != "")
                {
                    using (var fs = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fs.CopyToAsync(ms);
                            returnUser.image = File(ms.ToArray(), "image/jpg", fileDownloadName: returnUser.id + ".jpg");
                            fs.Close();
                        }
                    }
                }
                else
                    returnUser.image = null;

                currpResponse.User = returnUser;
                postResponses.Add(currpResponse);
            }

            return Ok(postResponses);
        }



        [HttpPost("addlike")]
        public async Task<ActionResult> addLike([FromBody] object data)
        {
            var objetoDeserializado = JObject.Parse(data.ToString());
            int id = (int)objetoDeserializado["id"];

            var postC = _context.Posts.FirstOrDefault(p => p.Id == id);
            postC.Likes++;

            _context.Posts.Entry(postC).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("addcomment")]
        public async Task<ActionResult<List<Comentario>>> addComment(ComentarioDto c)
        {
            Comentario comentario = new Comentario();
            comentario.Content = c.content;
            comentario.PostId = c.postid;
            comentario.UserId = c.userid;

            var postC = _context.Posts.FirstOrDefault(p => p.Id == c.postid);
            postC.Comentarios.Add(comentario);
            _context.Posts.Entry(postC).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var commentsList = _context.Comentarios.Where(c => c.PostId == c.PostId).ToList();
            foreach (var item in commentsList)
            {
                item.Since = _userService.TiempoTranscurrido(item.Date);
            }

            return Ok(postC.Comentarios);
        }
    }
}
