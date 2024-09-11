using EasyAnonymousForum.Server.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyAnonymousForum.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        public DbSet<ForumThread> Threads { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Topic> Topics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Shadow Properties
            modelBuilder.Entity<Topic>()
                .Property<Instant?>("DateCreated")
                .HasDefaultValueSql("now()");

            // Defaults
            modelBuilder.Entity<ForumThread>()
                .Property(e => e.DatePosted)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Comment>()
                .Property(e => e.DatePosted)
                .HasDefaultValueSql("now()");

            // Indexes
            modelBuilder.Entity<Topic>()
                .HasIndex(e => new { e.Name })
                .HasMethod("GIN")
                .IsTsVectorExpressionIndex("english");

            modelBuilder.Entity<ForumThread>()
                .HasIndex(e => new { e.Title })
                .HasMethod("GIN")
                .IsTsVectorExpressionIndex("english");

            modelBuilder.Entity<Comment>()
                .HasIndex(e => new { e.Content })
                .HasMethod("GIN")
                .IsTsVectorExpressionIndex("english");

            // Relationships
            modelBuilder.Entity<ForumThread>()
                .HasMany<Comment>()
                .WithOne(e => e.Thread)
                .IsRequired();

            modelBuilder.Entity<Topic>()
                .HasMany<ForumThread>()
                .WithOne(e => e.Topic)
                .IsRequired(false);
        }
    }
}
