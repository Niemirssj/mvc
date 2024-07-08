using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Data
{
    public class MvcMovieContext : DbContext
    {
        public MvcMovieContext(DbContextOptions<MvcMovieContext> options)
            : base(options)
        {
        }

        // Użycie pełnej nazwy typu w generycznym DbSet z domyślną wartością
        public DbSet<MvcMovie.Models.Movie> Movie { get; set; } = default!;

        // Użycie skróconej nazwy typu w generycznym DbSet
       

        // Inne DbSety dla pozostałych modeli
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<Monster> Monsters { get; set; }
        public DbSet<Training> Trainings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dodaj tutaj konfiguracje modeli, jeśli to konieczne
            // Na przykład:
            // modelBuilder.Entity<Movie>().ToTable("Movie");
            // modelBuilder.Entity<UserAccount>().ToTable("UserAccount");
            // itd.
        }
    }
}
