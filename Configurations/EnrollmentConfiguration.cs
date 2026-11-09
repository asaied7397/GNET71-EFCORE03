using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations;

public class EnrollmentConfiguration
    : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", table =>
        {
            table.HasCheckConstraint(
                "CK_Enrollments_Progress",
                "[ProgressPercentage] BETWEEN 0 AND 100"
            );

            table.HasCheckConstraint(
                "CK_Enrollments_Status",
                "[Status] IN (1, 2, 3)"
            );

            table.HasCheckConstraint(
                "CK_Enrollments_Completion",
                "([ProgressPercentage] = 100 AND [Status] = 2) OR " +
                "([ProgressPercentage] < 100 AND [Status] IN (1, 3))"
            );

            table.HasCheckConstraint(
                "CK_Enrollments_Grade",
                "[FinalGrade] IS NULL OR " +
                "([Status] = 2 AND [FinalGrade] BETWEEN 0 AND 100)"
            );
        });

        builder.HasKey(x => new
        {
            x.StudentId,
            x.CourseId
        });

        builder.Property(x => x.EnrollmentDate)
            .IsRequired();

        builder.Property(x => x.ProgressPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FinalGrade)
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Course)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}