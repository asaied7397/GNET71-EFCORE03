using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations;

public class InstructorConfiguration
    : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Bio)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.JoinedDate)
            .IsRequired();


        builder.HasData(
            new Instructor
            {
                Id = 1,
                FullName = "Robert Martin",
                Email = "robert@example.com",
                Bio = "Backend instructor",
                JoinedDate = new DateTime(2025, 1, 10)
            },
            new Instructor
            {
                Id = 2,
                FullName = "Sara Hassan",
                Email = "sara.instructor@example.com",
                Bio = null,
                JoinedDate = new DateTime(2025, 2, 15)
            },
            new Instructor
            {
                Id = 3,
                FullName = "Omar Ali",
                Email = "omar.instructor@example.com",
                Bio = "Data instructor",
                JoinedDate = new DateTime(2025, 3, 20)
            }
        );
    }
}