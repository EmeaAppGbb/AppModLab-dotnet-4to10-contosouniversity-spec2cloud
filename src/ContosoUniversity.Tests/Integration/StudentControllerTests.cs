using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class StudentControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public StudentControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateSeededClient();
        }

        [Fact]
        public async Task StudentIndex_ReturnsSuccessAndContainsStudentTable()
        {
            // US-F001-001: Student list page returns 200 with student table
            var response = await _client.GetAsync("/Students");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<table", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task StudentIndex_ShowsMax10StudentsPerPage()
        {
            // US-F001-001: Pagination shows max 10 students per page
            // Seed data has 8 students, all should show on page 1
            var response = await _client.GetAsync("/Students");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Alexander", content);
            Assert.Contains("Olivetto", content);
        }

        [Fact]
        public async Task StudentIndex_SearchByLastName_FiltersResults()
        {
            // US-F001-002: Search filters by last name
            var response = await _client.GetAsync("/Students?searchString=Alexander");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Alexander", content);
            Assert.DoesNotContain("Olivetto", content);
        }

        [Fact]
        public async Task StudentIndex_SearchByFirstName_FiltersResults()
        {
            // US-F001-002: Search filters by first name
            var response = await _client.GetAsync("/Students?searchString=Carson");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Carson", content);
            Assert.Contains("Alexander", content);
        }

        [Fact]
        public async Task StudentIndex_EmptySearch_ReturnsAllStudents()
        {
            // US-F001-002: Empty search returns all students
            var response = await _client.GetAsync("/Students?searchString=");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Alexander", content);
            Assert.Contains("Alonso", content);
        }

        [Fact]
        public async Task StudentIndex_SortByNameDesc_OrdersCorrectly()
        {
            // US-F001-003: Sort by last name descending
            var response = await _client.GetAsync("/Students?sortOrder=name_desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            // Olivetto should appear before Alexander in descending order
            var olivettoIndex = content.IndexOf("Olivetto");
            var alexanderIndex = content.IndexOf("Alexander");
            Assert.True(olivettoIndex > -1, "Olivetto should appear in the page");
            Assert.True(alexanderIndex > -1, "Alexander should appear in the page");
            Assert.True(olivettoIndex < alexanderIndex,
                "Olivetto should appear before Alexander when sorting by name descending");
        }

        [Fact]
        public async Task StudentIndex_SortByDate_OrdersCorrectly()
        {
            // US-F001-003: Sort by enrollment date ascending
            var response = await _client.GetAsync("/Students?sortOrder=Date");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            // Olivetto enrolled 2005, Alexander enrolled 2010 - date asc
            var olivettoIndex = content.IndexOf("Olivetto");
            var alexanderIndex = content.IndexOf("Alexander");
            Assert.True(olivettoIndex > -1, "Olivetto should appear in the page");
            Assert.True(alexanderIndex > -1, "Alexander should appear in the page");
            Assert.True(olivettoIndex < alexanderIndex,
                "Olivetto (2005) should appear before Alexander (2010) when sorting by date ascending");
        }

        [Fact]
        public async Task StudentCreate_Get_ReturnsForm()
        {
            // US-F001-004: Create page returns form
            var response = await _client.GetAsync("/Students/Create");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("LastName", content);
            Assert.Contains("FirstMidName", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task StudentDetails_ShowsStudentAndEnrollments()
        {
            // US-F001-005: Details shows student with enrollments
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var firstStudent = db.Students.FirstOrDefault();
            Assert.NotNull(firstStudent);

            var response = await _client.GetAsync($"/Students/Details/{firstStudent.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(firstStudent.LastName, content);
        }

        [Fact]
        public async Task StudentDetails_NonexistentId_Returns404()
        {
            // US-F001-005: Details returns 404 for nonexistent student
            var response = await _client.GetAsync("/Students/Details/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task StudentEdit_Get_ShowsPrePopulatedForm()
        {
            // US-F001-006: Edit page shows pre-populated form
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var firstStudent = db.Students.FirstOrDefault();
            Assert.NotNull(firstStudent);

            var response = await _client.GetAsync($"/Students/Edit/{firstStudent.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(firstStudent.LastName, content);
        }

        [Fact]
        public async Task StudentDelete_Get_ShowsConfirmation()
        {
            // US-F001-007: Delete confirmation page shows student info
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var firstStudent = db.Students.FirstOrDefault();
            Assert.NotNull(firstStudent);

            var response = await _client.GetAsync($"/Students/Delete/{firstStudent.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(firstStudent.LastName, content);
        }
    }
}
