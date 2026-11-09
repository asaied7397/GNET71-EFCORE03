using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Operations;

namespace OnlineLearningPlatform;

public static class Program
{
    private static int failures;

    public static int Main(string[] args)
    {
        string? connectionString =
            Environment.GetEnvironmentVariable(
                "LEARNING_CONNECTION_STRING"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine(
                "Set LEARNING_CONNECTION_STRING in this terminal before running."
            );

            return 1;
        }

        var options = new DbContextOptionsBuilder<LearningContext>()
            .UseSqlServer(connectionString)
            .Options;

        try
        {
            using var db = new LearningContext(options);

            db.Database.Migrate();

            Console.WriteLine(
                "Migrations applied successfully."
            );

            SeedStudents(db);
            SeedEnrollments(db);
            PrintCounts(db);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Database setup failed: {ex.GetBaseException().Message}"
            );

            return 1;
        }

        if (args.Contains("--seed-only"))
        {
            return 0;
        }

        int ahmedId;
        int saraId;

        using (var db = new LearningContext(options))
        {
            ahmedId = db.Students
                .Where(x => x.Email == "ahmed.seed@example.com")
                .Select(x => x.Id)
                .FirstOrDefault();

            saraId = db.Students
                .Where(x => x.Email == "sara.seed@example.com")
                .Select(x => x.Id)
                .FirstOrDefault();
        }

        int? newInstructorId = null;
        int? newCourseId = null;

        // ================= CREATE =================

        Run("CREATE: instructor", db =>
        {
            newInstructorId = PlatformOperations.AddInstructor(
                db,
                "Demo Instructor",
                "instructor.demo@example.com",
                "Created by the CRUD demonstration."
            );
        });

        Run("CREATE: course", db =>
        {
            if (newInstructorId is not int instructorId)
            {
                Console.WriteLine(
                    "Skipped: demo instructor unavailable."
                );

                return;
            }

            newCourseId = PlatformOperations.AddCourse(
                db,
                instructorId,
                3,
                "Demo: SQL Server Performance",
                120m,
                CourseLevel.Intermediate,
                true
            );
        });

        Run(
            "CREATE: three students",
            PlatformOperations.AddSeveralStudents
        );

        Run("CREATE: enroll student", db =>
        {
            if (newCourseId is int courseId)
            {
                PlatformOperations.EnrollStudent(
                    db,
                    ahmedId,
                    courseId
                );
            }
            else
            {
                Console.WriteLine(
                    "Skipped: demo course unavailable."
                );
            }
        });

        Run("CREATE validation: duplicate enrollment", db =>
        {
            if (newCourseId is int courseId)
            {
                PlatformOperations.EnrollStudent(
                    db,
                    ahmedId,
                    courseId
                );
            }
        });

        Run("CREATE validation: unpublished course", db =>
        {
            PlatformOperations.EnrollStudent(db, ahmedId, 6);
        });

        Run("CREATE validation: missing instructor", db =>
        {
            PlatformOperations.AddCourse(
                db,
                -1,
                3,
                "Invalid instructor demo",
                50m,
                CourseLevel.Beginner,
                false
            );
        });

        Run("CREATE validation: missing category", db =>
        {
            PlatformOperations.AddCourse(
                db,
                newInstructorId ?? -1,
                -1,
                "Invalid category demo",
                50m,
                CourseLevel.Beginner,
                false
            );
        });

        Run("CREATE validation: missing student", db =>
        {
            PlatformOperations.EnrollStudent(db, -1, 1);
        });

        Run("CREATE validation: missing course", db =>
        {
            PlatformOperations.EnrollStudent(db, ahmedId, -1);
        });

        // ================= READ =================

        Run("READ: find a course", db =>
        {
            PlatformOperations.FindCourse(
                db,
                1,
                "ASP.NET APIs"
            );
        });

        Run("READ: not found", db =>
        {
            PlatformOperations.FindCourse(
                db,
                -1,
                "Missing course"
            );
        });

        Run("READ: price filter", db =>
        {
            PlatformOperations.FilterCourses(db, 50m, 200m);
        });

        Run("READ: direct sub-categories and roots", db =>
        {
            PlatformOperations.PrintSubCategories(db, 1);
        });

        Run("READ: course roster", db =>
        {
            PlatformOperations.PrintCourseRoster(db, 1);
        });

        // ================= UPDATE =================

        Run("UPDATE: price", db =>
        {
            PlatformOperations.ChangePrice(db, 2, 175m);
        });

        Run("UPDATE: progress", db =>
        {
            PlatformOperations.UpdateProgress(
                db,
                ahmedId,
                1,
                100m
            );
        });

        Run("UPDATE validation: invalid progress", db =>
        {
            PlatformOperations.UpdateProgress(
                db,
                ahmedId,
                1,
                101m
            );
        });

        Run("UPDATE: publish", db =>
        {
            PlatformOperations.PublishCourse(db, 5);
        });

        // ================= DELETE =================

        Run("DELETE: unenroll", db =>
        {
            PlatformOperations.Unenroll(db, saraId, 3);
        });

        Run("DELETE: category without pre-check", db =>
        {
            PlatformOperations.DeleteCategoryUnsafe(db, 1);
        });

        Run("DELETE: category with pre-check", db =>
        {
            PlatformOperations.DeleteCategorySafe(db, 1);
        });

        Run("DELETE: course and cascading enrollments", db =>
        {
            if (newCourseId is int courseId)
            {
                PlatformOperations.DeleteCourse(db, courseId);
            }
            else
            {
                Console.WriteLine(
                    "Skipped: demo course unavailable."
                );
            }
        });

        // ================= BULK =================

        Run("BULK: published Intermediate prices +10%", db =>
        {
            PlatformOperations.BulkRaisePrices(
                db,
                CourseLevel.Intermediate
            );
        });

        Run(
            "BULK: delete Dropped enrollments",
            PlatformOperations.BulkDeleteDropped
        );

        Run("FINAL COUNTS", PrintCounts);

        return failures == 0 ? 0 : 1;

        // Fresh context for every operation.
        void Run(
            string title,
            Action<LearningContext> operation)
        {
            Console.WriteLine(
                $"\n===== {title} ====="
            );

            try
            {
                using var db = new LearningContext(options);

                operation(db);
            }
            catch (DbUpdateException ex)
            {
                failures++;

                Console.WriteLine(
                    $"Operation could not be saved: " +
                    $"{ex.GetBaseException().Message}"
                );
            }
            catch (Exception ex)
            {
                failures++;

                Console.WriteLine(
                    $"Operation could not be completed: {ex.Message}"
                );
            }
        }
    }

    private static void SeedStudents(LearningContext db)
    {
        if (db.Students.Any())
        {
            Console.WriteLine(
                "Students table is not empty; student seed skipped."
            );

            return;
        }

        db.Students.AddRange(
            new Student
            {
                FullName = "Ahmed Saied",
                Email = "ahmed.seed@example.com",
                DateOfBirth = new DateTime(2000, 1, 10)
            },
            new Student
            {
                FullName = "Sara Ali",
                Email = "sara.seed@example.com",
                DateOfBirth = new DateTime(2001, 2, 15)
            },
            new Student
            {
                FullName = "Youssef Omar",
                Email = "youssef.seed@example.com",
                DateOfBirth = new DateTime(1999, 3, 20)
            },
            new Student
            {
                FullName = "Mariam Hassan",
                Email = "mariam.seed@example.com",
                DateOfBirth = new DateTime(2002, 4, 25)
            },
            new Student
            {
                FullName = "Khalid Adel",
                Email = "khalid.seed@example.com",
                DateOfBirth = new DateTime(2000, 5, 5)
            }
        );

        db.SaveChanges();

        Console.WriteLine(
            "Seeded 5 students from Program.cs."
        );
    }

    private static void SeedEnrollments(LearningContext db)
    {
        if (db.Enrollments.Any())
        {
            Console.WriteLine(
                "Enrollments table is not empty; enrollment seed skipped."
            );

            return;
        }

        string[] emails =
        [
            "ahmed.seed@example.com",
            "sara.seed@example.com",
            "youssef.seed@example.com",
            "mariam.seed@example.com",
            "khalid.seed@example.com"
        ];

        var students = db.Students
            .Where(x => emails.Contains(x.Email))
            .Select(x => new
            {
                x.Id,
                x.Email
            })
            .ToList();

        bool missingOrAmbiguousStudent = emails.Any(
            email => students.Count(x => x.Email == email) != 1
        );

        if (missingOrAmbiguousStudent)
        {
            Console.WriteLine(
                "Enrollment seed skipped: expected demo students " +
                "are missing or ambiguous."
            );

            return;
        }

        int[] courseIds = [1, 2, 3, 4];

        int publishedCourseCount = db.Courses.Count(
            x => courseIds.Contains(x.Id) &&
                 x.IsPublished
        );

        if (publishedCourseCount != courseIds.Length)
        {
            Console.WriteLine(
                "Enrollment seed skipped: expected published seed " +
                "courses are missing."
            );

            return;
        }

        int StudentId(int index)
        {
            return students.Single(
                x => x.Email == emails[index]
            ).Id;
        }

        var date = new DateTime(
            2026,
            9,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc
        );

        Enrollment Row(
            int studentIndex,
            int courseId,
            decimal progress,
            EnrollmentStatus status,
            decimal? grade = null)
        {
            return new Enrollment
            {
                StudentId = StudentId(studentIndex),
                CourseId = courseId,
                EnrollmentDate = date,
                ProgressPercentage = progress,
                Status = status,
                FinalGrade = grade
            };
        }

        db.Enrollments.AddRange(
            Row(0, 1, 20, EnrollmentStatus.Active),
            Row(0, 2, 100, EnrollmentStatus.Completed, 88),
            Row(1, 1, 100, EnrollmentStatus.Completed, 92),
            Row(1, 3, 40, EnrollmentStatus.Active),
            Row(2, 2, 15, EnrollmentStatus.Dropped),
            Row(2, 4, 10, EnrollmentStatus.Active),
            Row(3, 3, 100, EnrollmentStatus.Completed, 95),
            Row(4, 4, 30, EnrollmentStatus.Dropped)
        );

        db.SaveChanges();

        Console.WriteLine(
            "Seeded 8 enrollments: Active, Completed and Dropped."
        );
    }

    private static void PrintCounts(LearningContext db)
    {
        Console.WriteLine(
            $"Categories: {db.Categories.Count()}"
        );

        Console.WriteLine(
            $"Instructors: {db.Instructors.Count()}"
        );

        Console.WriteLine(
            $"Courses: {db.Courses.Count()}"
        );

        Console.WriteLine(
            $"Students: {db.Students.Count()}"
        );

        Console.WriteLine(
            $"Enrollments: {db.Enrollments.Count()}"
        );
    }
}