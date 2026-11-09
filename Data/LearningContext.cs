using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Data;

public class LearningContext : DbContext
{
    public LearningContext(
        DbContextOptions<LearningContext> options
    ) : base(options)
    {
    }

    public DbSet<Instructor> Instructors => Set<Instructor>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LearningContext).Assembly
        );
    }
}