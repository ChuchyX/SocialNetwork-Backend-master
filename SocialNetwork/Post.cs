namespace SocialNetwork
{
    public partial class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Picture { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;

        public int UserId { get; set; }

        public virtual ICollection<Comentario> Comentarios { get; } = new List<Comentario>();

        public virtual User User { get; set; } = null!;
    }
}
