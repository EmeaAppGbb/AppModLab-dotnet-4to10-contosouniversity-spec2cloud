using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class InstructorControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public InstructorControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateSeededClient();
        }

        [Fact]
        public async Task InstructorIndex_ShowsInstructorList()
        {
            // US-F003-001: Instructor list shows all instructors
            var response = await _client.GetAsync("/Instructors");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Abercrombie", content);
            Assert.Contains("Fakhouri", content);
            Assert.Contains("Harui", content);
        }

        [Fact]
        public async Task InstructorIndex_WithInstructorId_ShowsCourses()
        {
            // US-F003-001: Selecting instructor shows courses
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var instructor = db.Instructors.FirstOrDefault(i => i.LastName == "Abercrombie");
            Assert.NotNull(instructor);

            var response = await _client.GetAsync($"/Instructors?id={instructor.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Abercrombie", content);
            // Abercrombie teaches Composition and Literature
            Assert.Contains("Composition", content);
            Assert.Contains("Literature", content);
        }

        [Fact]
        public async Task InstructorCreate_Get_HasCourseCheckboxes()
        {
            // US-F003-002: Create page has course checkboxes
            var response = await _client.GetAsync("/Instructors/Create");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("input", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InstructorDetails_ShowsInstructorInfo()
        {
            // US-F003-004: Details shows instructor info
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var instructor = db.Instructors.FirstOrDefault();
            Assert.NotNull(instructor);

            var response = await _client.GetAsync($"/Instructors/Details/{instructor.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(instructor.LastName, content);
        }

        [Fact]
        public async Task InstructorDelete_Get_ShowsConfirmation()
        {
            // US-F003-005: Delete confirmation page
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var instructor = db.Instructors.FirstOrDefault();
            Assert.NotNull(instructor);

            var response = await _client.GetAsync($"/Instructors/Delete/{instructor.ID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(instructor.LastName, content);
        }
    }
}
