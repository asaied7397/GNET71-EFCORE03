namespace OnlineLearningPlatform.Models;

public class Instructor
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateTime JoinedDate { get; set; }

    public ICollection<Course> Courses { get; set; }
        = new List<Course>();
}