using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform.Operations;

public static class PlatformOperations
{
    private const decimal MaxPrice = 9_999_999_999.99m;

    // ================= CREATE =================

    public static int? AddInstructor(
        LearningContext db,
        string name,
        string email,
        string? bio)
    {
        if (!ValidText(name, 150) ||
            !ValidText(email, 200) ||
            bio?.Length > 1000)
        {
            Console.WriteLine(
                "Instructor refused: invalid name, email or bio length."
            );

            return null;
        }

        var existing = db.Instructors
            .FirstOrDefault(x => x.Email == email);

        if (existing is not null)
        {
            Console.WriteLine(
                $"Demo instructor already exists: Id={existing.Id}."
            );

            return existing.Id;
        }

        var instructor = new Instructor
        {
            FullName = name.Trim(),
            Email = email.Trim(),
            Bio = bio,
            JoinedDate = DateTime.UtcNow
        };

        db.Instructors.Add(instructor);
        db.SaveChanges();

        Console.WriteLine(
            $"Instructor inserted. Generated Id: {instructor.Id}"
        );

        return instructor.Id;
    }

    public static int? AddCourse(
        LearningContext db,
        int instructorId,
        int categoryId,
        string title,
        decimal price,
        CourseLevel level,
        bool published)
    {
        if (!ValidText(title, 200) ||
            !ValidPrice(price) ||
            !Enum.IsDefined(level))
        {
            Console.WriteLine(
                "Course refused: invalid title, price or level."
            );

            return null;
        }

        if (!db.Instructors.Any(x => x.Id == instructorId))
        {
            Console.WriteLine(
                $"Course refused: instructor {instructorId} not found."
            );

            return null;
        }

        if (!db.Categories.Any(x => x.Id == categoryId))
        {
            Console.WriteLine(
                $"Course refused: category {categoryId} not found."
            );

            return null;
        }

        var existing = db.Courses.FirstOrDefault(
            x => x.Title == title &&
                 x.InstructorId == instructorId
        );

        if (existing is not null)
        {
            Console.WriteLine(
                $"Demo course already exists: Id={existing.Id}."
            );

            return existing.Id;
        }

        var course = new Course
        {
            Title = title.Trim(),
            Price = price,
            Level = level,
            InstructorId = instructorId,
            CategoryId = categoryId,
            IsPublished = published,
            PublishedDate = published ? DateTime.UtcNow : null
        };

        db.Courses.Add(course);
        db.SaveChanges();

        Console.WriteLine(
            $"Course inserted. Generated Id: {course.Id}"
        );

        return course.Id;
    }

    public static void AddSeveralStudents(LearningContext db)
    {
        var students = new[]
        {
            new Student
            {
                FullName = "Mona Adel",
                Email = "demo.mona@example.com",
                DateOfBirth = new DateTime(2001, 2, 10)
            },
            new Student
            {
                FullName = "Omar Samir",
                Email = "demo.omar@example.com",
                DateOfBirth = new DateTime(2000, 7, 15)
            },
            new Student
            {
                FullName = "Nour Ali",
                Email = "demo.nour@example.com",
                DateOfBirth = new DateTime(2002, 9, 20)
            }
        };

        var emails = students
            .Select(x => x.Email)
            .ToArray();

        if (db.Students.Any(x => emails.Contains(x.Email)))
        {
            Console.WriteLine(
                "Demo students already exist; the three-row insert was skipped."
            );

            return;
        }

        db.Students.AddRange(students);
        db.SaveChanges();

        foreach (var student in students)
        {
            Console.WriteLine(
                $"Student inserted: {student.FullName}; " +
                $"generated Id={student.Id}"
            );
        }
    }

    public static void EnrollStudent(
        LearningContext db,
        int studentId,
        int courseId)
    {
        if (!db.Students.Any(x => x.Id == studentId))
        {
            Console.WriteLine(
                $"Enrollment refused: student {studentId} not found."
            );

            return;
        }

        var course = db.Courses.Find(courseId);

        if (course is null)
        {
            Console.WriteLine(
                $"Enrollment refused: course {courseId} not found."
            );

            return;
        }

        if (!course.IsPublished)
        {
            Console.WriteLine(
                $"Enrollment refused: '{course.Title}' is unpublished."
            );

            return;
        }

        bool alreadyEnrolled = db.Enrollments.Any(
            x => x.StudentId == studentId &&
                 x.CourseId == courseId
        );

        if (alreadyEnrolled)
        {
            Console.WriteLine(
                "Enrollment refused: the student is already enrolled in this course."
            );

            return;
        }

        db.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            ProgressPercentage = 0,
            Status = EnrollmentStatus.Active,
            FinalGrade = null
        });

        db.SaveChanges();

        Console.WriteLine(
            $"Student {studentId} enrolled in course {courseId}."
        );
    }

    // ================= READ =================

    public static void FindCourse(
        LearningContext db,
        int id,
        string title)
    {
        var byId = db.Courses.Find(id);

        Console.WriteLine(
            byId is null
                ? $"Course Id {id}: not found."
                : $"Find: {byId.Id} | {byId.Title}"
        );

        if (!ValidText(title, 200))
        {
            Console.WriteLine(
                "Title search refused: provide a title of 1-200 characters."
            );

            return;
        }

        var byTitle = db.Courses
            .FirstOrDefault(x => x.Title == title);

        Console.WriteLine(
            byTitle is null
                ? $"Title '{title}': not found."
                : $"FirstOrDefault: {byTitle.Id} | {byTitle.Title}"
        );
    }

    public static void FilterCourses(
        LearningContext db,
        decimal minimum,
        decimal maximum)
    {
        if (!ValidPrice(minimum) ||
            !ValidPrice(maximum) ||
            minimum > maximum)
        {
            Console.WriteLine(
                "Invalid price range: use non-negative prices with minimum <= maximum."
            );

            return;
        }

        var courses = db.Courses
            .AsNoTracking()
            .Where(x => x.Price >= minimum && x.Price <= maximum)
            .OrderByDescending(x => x.Price)
            .Select(x => new
            {
                x.Title,
                x.Level,
                x.Price
            })
            .ToList();

        if (courses.Count == 0)
        {
            Console.WriteLine("No courses match the price range.");
        }

        foreach (var course in courses)
        {
            Console.WriteLine(
                $"{course.Title} | {course.Level} | {course.Price:F2}"
            );
        }
    }

    public static void PrintSubCategories(
        LearningContext db,
        int categoryId)
    {
        if (!db.Categories.Any(x => x.Id == categoryId))
        {
            Console.WriteLine(
                $"Category {categoryId}: not found."
            );
        }
        else
        {
            Console.WriteLine(
                $"Direct sub-categories of {categoryId}:"
            );

            var children = db.Categories
                .AsNoTracking()
                .Where(x => x.ParentCategoryId == categoryId)
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToList();

            if (children.Count == 0)
            {
                Console.WriteLine("No direct sub-categories.");
            }

            foreach (var name in children)
            {
                Console.WriteLine($"- {name}");
            }
        }

        Console.WriteLine("Top-level categories:");

        var roots = db.Categories
            .AsNoTracking()
            .Where(x => x.ParentCategoryId == null)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToList();

        foreach (var name in roots)
        {
            Console.WriteLine($"- {name}");
        }
    }

    public static void PrintCourseRoster(
        LearningContext db,
        int courseId)
    {
        if (!db.Courses.Any(x => x.Id == courseId))
        {
            Console.WriteLine(
                $"Course {courseId}: not found."
            );

            return;
        }

        var studentIds = db.Enrollments
            .Where(x => x.CourseId == courseId)
            .Select(x => x.StudentId)
            .ToList();

        Console.WriteLine(
            $"Enrollment records: {studentIds.Count} (all statuses)."
        );

        var names = db.Students
            .AsNoTracking()
            .Where(x => studentIds.Contains(x.Id))
            .OrderBy(x => x.FullName)
            .Select(x => x.FullName)
            .ToList();

        foreach (var name in names)
        {
            Console.WriteLine($"- {name}");
        }
    }

    // ================= UPDATE =================

    public static void ChangePrice(
        LearningContext db,
        int courseId,
        decimal price)
    {
        if (!ValidPrice(price))
        {
            Console.WriteLine(
                "Price refused: use a non-negative amount with at most " +
                "two decimal places within decimal(12,2)."
            );

            return;
        }

        var course = db.Courses.Find(courseId);

        if (course is null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        decimal oldPrice = course.Price;

        course.Price = price;

        db.SaveChanges();

        Console.WriteLine(
            $"{course.Title}: {oldPrice:F2} -> {course.Price:F2}"
        );
    }

    public static void UpdateProgress(
        LearningContext db,
        int studentId,
        int courseId,
        decimal progress)
    {
        if (progress < 0 ||
            progress > 100 ||
            decimal.Round(progress, 2) != progress)
        {
            Console.WriteLine(
                "Progress refused: use 0-100 with at most two decimal places."
            );

            return;
        }

        var enrollment = db.Enrollments.Find(studentId, courseId);

        if (enrollment is null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }

        enrollment.ProgressPercentage = progress;

        if (progress == 100)
        {
            enrollment.Status = EnrollmentStatus.Completed;
        }
        else
        {
            if (enrollment.Status == EnrollmentStatus.Completed)
            {
                enrollment.Status = EnrollmentStatus.Active;
            }

            enrollment.FinalGrade = null;
        }

        db.SaveChanges();

        Console.WriteLine(
            $"Progress: {progress:F2}%; status: {enrollment.Status}"
        );
    }

    public static void PublishCourse(
        LearningContext db,
        int courseId)
    {
        var course = db.Courses.Find(courseId);

        if (course is null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        if (course.IsPublished)
        {
            Console.WriteLine(
                "Course is already published; original date preserved."
            );

            return;
        }

        course.IsPublished = true;
        course.PublishedDate = DateTime.UtcNow;

        db.SaveChanges();

        Console.WriteLine(
            $"Published '{course.Title}' at " +
            $"{course.PublishedDate:yyyy-MM-dd HH:mm:ss} UTC."
        );
    }

    // ================= DELETE =================

    public static void Unenroll(
        LearningContext db,
        int studentId,
        int courseId)
    {
        var enrollment = db.Enrollments.Find(studentId, courseId);

        if (enrollment is null)
        {
            Console.WriteLine(
                "Unenroll skipped: enrollment not found."
            );

            return;
        }

        db.Enrollments.Remove(enrollment);
        db.SaveChanges();

        Console.WriteLine(
            $"Removed enrollment ({studentId}, {courseId})."
        );
    }

    public static void DeleteCategoryUnsafe(
        LearningContext db,
        int categoryId)
    {
        var category = db.Categories.Find(categoryId);

        if (category is null)
        {
            Console.WriteLine("Category not found.");
            return;
        }

        try
        {
            db.Categories.Remove(category);
            db.SaveChanges();

            Console.WriteLine(
                $"Category {categoryId} deleted."
            );
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(
                "Deletion blocked: a referenced category cannot be removed."
            );

            Console.WriteLine(
                $"DbUpdateException: {ex.Message}"
            );

            Console.WriteLine(
                $"Database message: {ex.GetBaseException().Message}"
            );

            db.Entry(category).State = EntityState.Unchanged;
        }
    }

    public static void DeleteCategorySafe(
        LearningContext db,
        int categoryId)
    {
        var category = db.Categories.Find(categoryId);

        if (category is null)
        {
            Console.WriteLine("Category not found.");
            return;
        }

        bool hasChildren = db.Categories.Any(
            x => x.ParentCategoryId == categoryId
        );

        if (hasChildren)
        {
            Console.WriteLine(
                "Cannot delete category: it still has sub-categories."
            );

            return;
        }

        bool hasCourses = db.Courses.Any(
            x => x.CategoryId == categoryId
        );

        if (hasCourses)
        {
            Console.WriteLine(
                "Cannot delete category: courses still belong to it."
            );

            return;
        }

        db.Categories.Remove(category);
        db.SaveChanges();

        Console.WriteLine(
            $"Category {categoryId} deleted safely."
        );
    }

    public static void DeleteCourse(
        LearningContext db,
        int courseId)
    {
        var course = db.Courses.Find(courseId);

        if (course is null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        int before = db.Enrollments.Count(
            x => x.CourseId == courseId
        );

        Console.WriteLine(
            $"Enrollments before deleting course: {before}"
        );

        db.Courses.Remove(course);
        db.SaveChanges();


        int after = db.Enrollments.Count(
            x => x.CourseId == courseId
        );

        Console.WriteLine(
            $"Enrollments after deleting course: {after}"
        );
    }

    // ================= BULK =================

    public static void BulkRaisePrices(
        LearningContext db,
        CourseLevel level)
    {
        if (!Enum.IsDefined(level))
        {
            Console.WriteLine("Invalid course level.");
            return;
        }

        int affected = db.Courses
            .Where(x => x.IsPublished && x.Level == level)
            .ExecuteUpdate(setters => setters
                .SetProperty(
                    x => x.Price,
                    x => x.Price * 1.10m
                )
            );

        Console.WriteLine(
            $"Published {level} courses raised by 10%. " +
            $"Affected rows: {affected}"
        );

    }

    public static void BulkDeleteDropped(LearningContext db)
    {
        int affected = db.Enrollments
            .Where(x => x.Status == EnrollmentStatus.Dropped)
            .ExecuteDelete();

        Console.WriteLine(
            $"Dropped enrollments deleted. Affected rows: {affected}"
        );

    }

    // ================= VALIDATION =================

    private static bool ValidText(string? value, int maximum)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length <= maximum;
    }

    private static bool ValidPrice(decimal value)
    {
        return value >= 0
            && value <= MaxPrice
            && decimal.Round(value, 2) == value;
    }
}