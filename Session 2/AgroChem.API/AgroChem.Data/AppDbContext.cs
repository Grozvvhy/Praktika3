using System.Data.Entity;
using AgroChem.Data.Models;

namespace AgroChem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeComponent> RecipeComponents { get; set; }
        public DbSet<TechnologicalCard> TechnologicalCards { get; set; }
        public DbSet<TechnologicalStep> TechnologicalSteps { get; set; }
        public DbSet<ProductionBatch> ProductionBatches { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<LabTestParameterResult> LabTestParameterResults { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Recipe>()
                .HasIndex(r => new { r.ProductId, r.Version })
                .IsUnique();

            modelBuilder.Entity<RecipeComponent>()
                .HasRequired(rc => rc.Recipe)
                .WithMany(r => r.Components)
                .HasForeignKey(rc => rc.RecipeId);

            modelBuilder.Entity<RecipeComponent>()
                .Property(c => c.Percentage)
                .HasPrecision(5, 2);
        }
    }
}