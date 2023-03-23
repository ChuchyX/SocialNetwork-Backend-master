namespace SocialNetwork
{
    public partial class Comentario
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;

        public int UserId { get; set; }

        public int PostId { get; set; }

        public virtual Post Post { get; set; } = null!;
    }
}
