using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class CourseControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public CourseControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateSeededClient();
        }

        [Fact]
        public async Task CourseIndex_ShowsCoursesWithDepartment()
        {
            // US-F002-001: Course list shows department info
            var response = await _client.GetAsync("/Courses");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Chemistry", content);
            Assert.Contains("Engineering", content);
        }

        [Fact]
        public async Task CourseCreate_Get_HasDepartmentDropdown()
        {
            // US-F002-002: Create page has department dropdown
            var response = await _client.GetAsync("/Courses/Create");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<select", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("DepartmentID", content);
        }

        [Fact]
        public async Task CourseDetails_ShowsCourseWithDepartment()
        {
            // US-F002-003: Details shows course info
            // Use a known course ID from the DB
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var course = db.Courses.FirstOrDefault();
            Assert.NotNull(course);

            var response = await _client.GetAsync($"/Courses/Details/{course.CourseID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(course.Title, content);
        }

        [Fact]
        public async Task CourseDetails_NonexistentId_Returns404()
        {
            // US-F002-003: Details returns 404 for nonexistent course
            var response = await _client.GetAsync("/Courses/Details/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CourseDelete_Get_ShowsConfirmation()
        {
            // US-F002-005: Delete confirmation shows course info
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var course = db.Courses.FirstOrDefault();
            Assert.NotNull(course);

            var response = await _client.GetAsync($"/Courses/Delete/{course.CourseID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(course.Title, content);
        }

        [Fact]
        public async Task CourseGrades_ShowsEnrolledStudents()
        {
            // F-009: Grades page shows enrolled students
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            // Find a course that has enrollments
            var courseWithEnrollments = db.Courses.FirstOrDefault(c => c.Enrollments.Any());
            Assert.NotNull(courseWithEnrollments);

            var response = await _client.GetAsync($"/Courses/Grades/{courseWithEnrollments.CourseID}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(courseWithEnrollments.Title, content);
        }

        [Fact]
        public async Task CourseGrades_NonexistentId_Returns404()
        {
            // F-009: Grades page returns 404 for nonexistent course
            var response = await _client.GetAsync("/Courses/Grades/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
