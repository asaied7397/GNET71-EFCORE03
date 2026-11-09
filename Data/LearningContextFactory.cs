using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnlineLearningPlatform.Data;

public class LearningContextFactory
    : IDesignTimeDbContextFactory<LearningContext>
{
    public LearningContext CreateDbContext(string[] args)
    {
        string? connectionString =
            Environment.GetEnvironmentVariable(
                "LEARNING_CONNECTION_STRING"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set LEARNING_CONNECTION_STRING in this terminal first."
            );
        }

        var options = new DbContextOptionsBuilder<LearningContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new LearningContext(options);
    }
}