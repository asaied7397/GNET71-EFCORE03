using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Configurations;

public class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", table =>
        {
            table.HasCheckConstraint(
                "CK_Categories_NotSelfParent",
                "[ParentCategoryId] IS NULL OR [ParentCategoryId] <> [Id]"
            );
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasData(
            new Category
            {
                Id = 1,
                Name = "Programming",
                Description = "Software development",
                ParentCategoryId = null
            },
            new Category
            {
                Id = 2,
                Name = "Backend",
                Description = "Server-side development",
                ParentCategoryId = 1
            },
            new Category
            {
                Id = 3,
                Name = "ASP.NET",
                Description = "ASP.NET applications",
                ParentCategoryId = 2
            },
            new Category
            {
                Id = 4,
                Name = "Frontend",
                Description = null,
                ParentCategoryId = 1
            },
            new Category
            {
                Id = 5,
                Name = "Data Science",
                Description = "Data analysis",
                ParentCategoryId = null
            }
        );

    }
}