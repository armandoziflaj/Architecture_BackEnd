namespace Sulozeqi_BackEnd.Models;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTranslation> ProjectTranslations { get; set; }
    public DbSet<ProjectPhoto> ProjectPhotos { get; set; }
    public DbSet<ContactInquiry> ContactInquiries { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Translations)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Category>().Property(c => c.RowVersion).IsRowVersion();
        modelBuilder.Entity<ContactInquiry>().Property(c => c.RowVersion).IsRowVersion();
        modelBuilder.Entity<Project>().Property(p => p.RowVersion).IsRowVersion();
        modelBuilder.Entity<ProjectTranslation>().Property(p => p.RowVersion).IsRowVersion();
        modelBuilder.Entity<ProjectPhoto>().Property(pp => pp.RowVersion).IsRowVersion();
    }
}

