using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations;

public class CourseConfiguration
    : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", table =>
        {
            table.HasCheckConstraint(
                "CK_Courses_Price",
                "[Price] >= 0"
            );

            table.HasCheckConstraint(
                "CK_Courses_Level",
                "[Level] IN (1, 2, 3)"
            );

            table.HasCheckConstraint(
                "CK_Courses_Publication",
                "([IsPublished] = 0 AND [PublishedDate] IS NULL) OR " +
                "([IsPublished] = 1 AND [PublishedDate] IS NOT NULL)"
            );
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.Price)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.PublishedDate)
            .IsRequired(false);

        builder.HasOne(x => x.Instructor)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasData(
            new Course
            {
                Id = 1,
                Title = "C# Fundamentals",
                Description = "Learn C#",
                Price = 80m,
                Level = CourseLevel.Beginner,
                IsPublished = true,
                PublishedDate = new DateTime(2026, 1, 10),
                InstructorId = 1,
                CategoryId = 2
            },
            new Course
            {
                Id = 2,
                Title = "ASP.NET APIs",
                Description = "Build APIs",
                Price = 150m,
                Level = CourseLevel.Intermediate,
                IsPublished = true,
                PublishedDate = new DateTime(2026, 2, 10),
                InstructorId = 1,
                CategoryId = 3
            },
            new Course
            {
                Id = 3,
                Title = "Frontend Foundations",
                Description = null,
                Price = 60m,
                Level = CourseLevel.Beginner,
                IsPublished = true,
                PublishedDate = new DateTime(2026, 3, 10),
                InstructorId = 2,
                CategoryId = 4
            },
            new Course
            {
                Id = 4,
                Title = "Advanced Data Analysis",
                Description = "Advanced analysis",
                Price = 220m,
                Level = CourseLevel.Advanced,
                IsPublished = true,
                PublishedDate = new DateTime(2026, 4, 10),
                InstructorId = 3,
                CategoryId = 5
            },
            new Course
            {
                Id = 5,
                Title = "Advanced EF Core",
                Description = "Advanced EF Core topics",
                Price = 180m,
                Level = CourseLevel.Advanced,
                IsPublished = false,
                PublishedDate = null,
                InstructorId = 2,
                CategoryId = 3
            },
            new Course
            {
                Id = 6,
                Title = "Python for Data",
                Description = null,
                Price = 110m,
                Level = CourseLevel.Intermediate,
                IsPublished = false,
                PublishedDate = null,
                InstructorId = 3,
                CategoryId = 5
            }
        );

    }
}