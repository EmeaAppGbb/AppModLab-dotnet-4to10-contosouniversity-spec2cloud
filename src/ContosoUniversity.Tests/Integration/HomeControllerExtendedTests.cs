using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class HomeControllerExtendedTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HomeControllerExtendedTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateSeededClient();
        }

        [Fact]
        public async Task About_ShowsEnrollmentStatistics()
        {
            // US-F006-001: About page shows enrollment statistics table
            var response = await _client.GetAsync("/Home/About");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Enrollment", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Contact_ShowsContactInformation()
        {
            // US-F007-001: Contact page shows address and phone
            var response = await _client.GetAsync("/Home/Contact");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Contact", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Error_ReturnsErrorPage()
        {
            // US-F008-001: Error page renders
            var response = await _client.GetAsync("/Home/Error");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
