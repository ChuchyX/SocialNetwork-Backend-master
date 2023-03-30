using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork.Services.UserService
{
    public interface IUserService
    {
        string GetMyEmail();
        Task<User> Register(UserRegisterDto request);
        Task<Tuple<ReturnUser, string>> GetMe();
        Task<Tuple<ResponseLogin, string>> Login(UserLoginDto request);
        Task<Tuple<ReturnUser, string>> UploadPP(IFormFile file);
        string TiempoTranscurrido(DateTime fecha);
    }
}
