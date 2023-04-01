using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SocialNetwork.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, DataContext context, IWebHostEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _context = context;
            _environment = env;
        }
        public string GetMyEmail()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            }
            return result;
        }
        public async Task<User> Register(UserRegisterDto request)
        {
            var listaUsers = _context.Users.ToList();
            if (listaUsers.Any(u => u.Email == request.Email && VerifyPasswordHash(request.Password, u.PasswordHash, u.PasswordSalt)))
            {
                return null;
            }
            else
            {
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                var user = new User();
                user.Email = request.Email;
                user.Username = request.Username;
                user.Edad = request.Edad;
                user.Sexo = request.Sexo;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
        }
        public async Task<Tuple<ReturnUser, string>> GetMe()
        {
            var email = GetMyEmail();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return null;
            }

            ReturnUser rUser = new ReturnUser();
            rUser.username = user.Username;
            rUser.email = user.Email;
            rUser.edad = user.Edad;
            rUser.sexo = user.Sexo;
            rUser.id = user.Id;
            string nameImg = user.ProfilePicture;

            return new Tuple<ReturnUser, string>(rUser, nameImg);
        }
        public async Task<Tuple<ResponseLogin, string>> Login(UserLoginDto request)
        {
            var listaUsers = _context.Users.ToList();
            var usuario = new User();
            bool encontrado = false;
            foreach (var user in listaUsers)
            {
                if (user.Email == request.Email && VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    usuario = user;
                    encontrado = true;
                }
            }
            if (!encontrado)
            {
                return null;
            }
            else
            {
                string token = CreateToken(usuario);
                usuario.RefreshToken = token;
                usuario.TokenCreated = DateTime.Now;
                usuario.TokenExpires = DateTime.Now.AddDays(7);

                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                ReturnUser rUser = new ReturnUser();
                rUser.username = usuario.Username;
                rUser.email = usuario.Email;
                rUser.edad = usuario.Edad;
                rUser.sexo = usuario.Sexo;
                rUser.id = usuario.Id;
                string nameImg = usuario.ProfilePicture;

                ResponseLogin response = new ResponseLogin();
                response.User = rUser;
                response.Token = token;

                return new Tuple<ResponseLogin, string>(response, nameImg);
            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(User user)
        {
            string role = "User";
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        public async Task<Tuple<ReturnUser, string>> UploadPP(IFormFile file)
        {
            var email = GetMyEmail();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            string nameImg = "u-" + user.Id;
            user.ProfilePicture = nameImg;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var filename = Path.Combine(_environment.ContentRootPath, "uploadPictures", nameImg);
            var fs = new FileStream(filename, FileMode.Create);
            await file.CopyToAsync(fs);
            fs.Close();
            ReturnUser rUser = new ReturnUser();
            rUser.username = user.Username;
            rUser.email = user.Email;
            rUser.edad = user.Edad;
            rUser.sexo = user.Sexo;
            rUser.id = user.Id;

            return new Tuple<ReturnUser, string>(rUser, nameImg);
        }
        public string TiempoTranscurrido(DateTime fecha)
        {
            TimeSpan diferencia = DateTime.Now - fecha;
            if (diferencia.TotalSeconds < 60)
            {
                return "Ahora";
            }
            else if (diferencia.TotalMinutes < 60)
            {
                return "Hace " + ((int)diferencia.TotalMinutes).ToString() + " minutos";
            }
            else if (diferencia.TotalHours < 24)
            {
                int horas = (int)diferencia.TotalHours;
                return "Hace " + horas + (horas == 1 ? " hora" : " horas");
            }
            else if (diferencia.TotalDays < 30)
            {
                return "Hace " + ((int)diferencia.TotalDays).ToString() + " días";
            }
            else if (diferencia.TotalDays < 365)
            {
                int meses = (int)diferencia.TotalDays / 30;
                return "Hace " + meses.ToString() + (meses == 1 ? " mes" : " meses");
            }
            else
            {
                int años = (int)diferencia.TotalDays / 365;
                return "Hace " + años.ToString() + (años == 1 ? " año" : " años");
            }
        }
    }
}
