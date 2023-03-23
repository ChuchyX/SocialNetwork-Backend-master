using Microsoft.EntityFrameworkCore;

namespace SocialNetwork
{
    public partial class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public virtual DbSet<Comentario> Comentarios { get; set; }

        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasIndex(e => e.PostId, "IX_Comentarios_PostId");

                entity.HasOne(d => d.Post).WithMany(p => p.Comentarios).HasForeignKey(d => d.PostId);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_Posts_UserId");

                entity.HasOne(d => d.User).WithMany(p => p.Posts).HasForeignKey(d => d.UserId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
