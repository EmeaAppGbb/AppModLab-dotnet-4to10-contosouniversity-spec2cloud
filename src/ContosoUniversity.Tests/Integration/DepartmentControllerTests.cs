using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class DepartmentControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public DepartmentControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateSeededClient();
        }

        [Fact]
        public async Task DepartmentIndex_ShowsDepartmentsWithAdmin()
        {
            // US-F004-001: Department list shows all departments with admin
            var response = await _client.GetAsync("/Departments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("English", content);
            Assert.Contains("Mathematics", content);
            Assert.Contains("Engineering", content);
            Assert.Contains("Economics", content);
        }

        [Fact]
        public async Task DepartmentCreate_Get_HasInstructorDropdown()
        {
            // US-F004-002: Create page has instructor dropdown
            var response = await _client.GetAsync("/Departments/Create");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<form", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<select", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("InstructorID", content);
        }

        [Fact]
        public async Task DepartmentDetails_ShowsDepartmentInfo()
        {
            // US-F004-004: Details shows department info
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var dept = db.Departments.FirstOrDefault();
            Assert.NotNull(dept);

            var response = await _client.GetAsync($"/Departments/Details/{dept.DepartmentID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(dept.Name, content);
        }

        [Fact]
        public async Task DepartmentDelete_Get_ShowsConfirmation()
        {
            // US-F004-005: Delete confirmation shows department info
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var dept = db.Departments.FirstOrDefault();
            Assert.NotNull(dept);

            var response = await _client.GetAsync($"/Departments/Delete/{dept.DepartmentID}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(dept.Name, content);
        }
    }
}
