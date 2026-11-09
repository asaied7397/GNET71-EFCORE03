# Online Learning Platform

EF Core Code-First console application using .NET 10 and SQL Server.

## Features

- Fluent API configuration in a separate class for each entity.
- Instructor–Course and Category–Course relationships.
- Self-referencing category hierarchy with restricted parent deletion.
- Student–Course many-to-many relationship through Enrollment.
- Composite enrollment primary key.
- Separate model and seed-data migrations.
- Conditional runtime seeding for students and enrollments.
- CRUD operations with validation and friendly messages.
- Bulk updates and deletes without loading entities.
- No Include, lazy loading or joins.

## Run

Set `LEARNING_CONNECTION_STRING` in the terminal, then run:

```bash
dotnet restore
dotnet ef database update
dotnet run -- --seed-only
dotnet run
```

## Migrations

- `InitialModel`: tables, relationships and constraints.
- `SeedCatalog`: categories, instructors and courses.

## Notes

- Final grades use an assumed 0–100 scale.
- The full demonstration modifies and deletes data.
- The first parent-category deletion intentionally fails and is caught.
- Keep migrations in source control and exclude bin/obj.
