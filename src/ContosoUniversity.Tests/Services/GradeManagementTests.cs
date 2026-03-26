using System.Threading.Tasks;
using System.Linq;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContosoUniversity.Tests.Services
{
    public class GradeManagementTests
    {
        private SchoolContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new SchoolContext(options);
        }

        private void SeedCourseWithEnrollments(SchoolContext context)
        {
            var student = new Student
            {
                FirstMidName = "Test",
                LastName = "Student",
                EnrollmentDate = new System.DateTime(2024, 9, 1)
            };
            context.Students.Add(student);
            context.SaveChanges();

            var department = new Department
            {
                Name = "Test Dept",
                Budget = 100000,
                StartDate = new System.DateTime(2020, 1, 1)
            };
            context.Departments.Add(department);
            context.SaveChanges();

            var course = new Course
            {
                CourseID = 1001,
                Title = "Test Course",
                Credits = 3,
                DepartmentID = department.DepartmentID
            };
            context.Courses.Add(course);
            context.SaveChanges();

            var enrollment = new Enrollment
            {
                StudentID = student.ID,
                CourseID = course.CourseID,
                Grade = null
            };
            context.Enrollments.Add(enrollment);
            context.SaveChanges();
        }

        [Fact]
        public async Task GradeUpdate_SavesGradeToDatabase()
        {
            using var context = CreateContext();
            SeedCourseWithEnrollments(context);

            var enrollment = await context.Enrollments.FirstAsync();
            Assert.Null(enrollment.Grade);

            enrollment.Grade = Grade.A;
            await context.SaveChangesAsync();

            var updated = await context.Enrollments.FindAsync(enrollment.EnrollmentID);
            Assert.Equal(Grade.A, updated.Grade);
        }

        [Fact]
        public async Task GradeUpdate_CanClearGrade()
        {
            using var context = CreateContext();
            SeedCourseWithEnrollments(context);

            var enrollment = await context.Enrollments.FirstAsync();
            enrollment.Grade = Grade.B;
            await context.SaveChangesAsync();

            enrollment.Grade = null;
            await context.SaveChangesAsync();

            var updated = await context.Enrollments.FindAsync(enrollment.EnrollmentID);
            Assert.Null(updated.Grade);
        }

        [Fact]
        public async Task BatchGradeUpdate_UpdatesMultipleEnrollments()
        {
            using var context = CreateContext();

            var student1 = new Student { FirstMidName = "Alice", LastName = "A", EnrollmentDate = new System.DateTime(2024, 9, 1) };
            var student2 = new Student { FirstMidName = "Bob", LastName = "B", EnrollmentDate = new System.DateTime(2024, 9, 1) };
            context.Students.AddRange(student1, student2);
            await context.SaveChangesAsync();

            var dept = new Department { Name = "Dept", Budget = 50000, StartDate = new System.DateTime(2020, 1, 1) };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            var course = new Course { CourseID = 2001, Title = "Batch Test", Credits = 3, DepartmentID = dept.DepartmentID };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var e1 = new Enrollment { StudentID = student1.ID, CourseID = 2001, Grade = null };
            var e2 = new Enrollment { StudentID = student2.ID, CourseID = 2001, Grade = null };
            context.Enrollments.AddRange(e1, e2);
            await context.SaveChangesAsync();

            // Batch update
            e1.Grade = Grade.A;
            e2.Grade = Grade.C;
            await context.SaveChangesAsync();

            var results = await context.Enrollments.Where(e => e.CourseID == 2001).ToListAsync();
            Assert.Equal(Grade.A, results.First(e => e.StudentID == student1.ID).Grade);
            Assert.Equal(Grade.C, results.First(e => e.StudentID == student2.ID).Grade);
        }
    }
}
