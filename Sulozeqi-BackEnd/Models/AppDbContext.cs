namespace Sulozeqi_BackEnd.Models;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectPhoto> ProjectPhotos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Category>().Property(c => c.RowVersion).IsRowVersion();
        modelBuilder.Entity<Project>().Property(p => p.RowVersion).IsRowVersion();
        modelBuilder.Entity<ProjectPhoto>().Property(pp => pp.RowVersion).IsRowVersion();
    }
}

