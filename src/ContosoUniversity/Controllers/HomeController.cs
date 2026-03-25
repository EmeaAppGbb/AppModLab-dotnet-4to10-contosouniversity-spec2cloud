using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Data;
using ContosoUniversity.Models.SchoolViewModels;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(SchoolContext context, INotificationService notificationService, ILogger<HomeController> logger)
            : base(context, notificationService, logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            IQueryable<EnrollmentDateGroup> data =
                from student in db.Students
                group student by student.EnrollmentDate into dateGroup
                select new EnrollmentDateGroup()
                {
                    EnrollmentDate = dateGroup.Key,
                    StudentCount = dateGroup.Count()
                };
            return View(await data.ToListAsync());
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public new IActionResult Unauthorized()
        {
            ViewBag.Message = "You don't have permission to access this resource.";
            return View();
        }
    }
}
