using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Data
{
    public class RecipesDbContext : DbContext
    {
        public RecipesDbContext(DbContextOptions<RecipesDbContext> options) : base(options) { }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SavedRecipe> SavedRecipes { get; set; }
        public DbSet<Comments> Comments { get; set; }

        protected override  void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().ToTable("category");
            modelBuilder.Entity<Recipe>().ToTable("recipes");
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<SavedRecipe>().ToTable("saved_recipes");
            modelBuilder.Entity<Comments>().ToTable("rewiews");

            modelBuilder.Entity<SavedRecipe>().HasIndex(sr => new { sr.userID, sr.recipeID }).IsUnique();

            modelBuilder.Entity<Comments>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.userID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}