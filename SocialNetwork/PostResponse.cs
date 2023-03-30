using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork
{
    public class PostResponse
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public FileContentResult Image { get; set; }
        public DateTime Date { get; set; }
        public string Since { get; set; }
        public int Likes { get; set; }
        public  List<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ReturnUser User { get; set; }
    }
}
