using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : BaseController
    {
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(SchoolContext context, INotificationService notificationService, ILogger<StudentsController> logger)
            : base(context, notificationService, logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var students = from s in db.Students select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(await PaginatedList<Student>.CreateAsync(students, pageNumber, pageSize));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Student student = await db.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Where(s => s.ID == id).SingleAsync();
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult Create()
        {
            var student = new Student
            {
                EnrollmentDate = DateTime.Today
            };
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (student.EnrollmentDate == DateTime.MinValue || student.EnrollmentDate == default(DateTime))
                {
                    ModelState.AddModelError("EnrollmentDate", "Please enter a valid enrollment date.");
                }

                if (student.EnrollmentDate < new DateTime(1753, 1, 1) || student.EnrollmentDate > new DateTime(9999, 12, 31))
                {
                    ModelState.AddModelError("EnrollmentDate", "Enrollment date must be between 1753 and 9999.");
                }

                if (ModelState.IsValid)
                {
                    db.Students.Add(student);
                    await db.SaveChangesAsync();

                    var studentName = $"{student.FirstMidName} {student.LastName}";
                    await SendEntityNotificationAsync("Student", student.ID.ToString(), studentName, EntityOperation.CREATE);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating student: {ex.Message}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (student.EnrollmentDate == DateTime.MinValue || student.EnrollmentDate == default(DateTime))
                {
                    ModelState.AddModelError("EnrollmentDate", "Please enter a valid enrollment date.");
                }

                if (student.EnrollmentDate < new DateTime(1753, 1, 1) || student.EnrollmentDate > new DateTime(9999, 12, 31))
                {
                    ModelState.AddModelError("EnrollmentDate", "Enrollment date must be between 1753 and 9999.");
                }

                if (ModelState.IsValid)
                {
                    db.Entry(student).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    var studentName = $"{student.FirstMidName} {student.LastName}";
                    await SendEntityNotificationAsync("Student", student.ID.ToString(), studentName, EntityOperation.UPDATE);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error editing student: {ex.Message}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Student student = await db.Students.FindAsync(id);
                var studentName = $"{student.FirstMidName} {student.LastName}";
                db.Students.Remove(student);
                await db.SaveChangesAsync();

                await SendEntityNotificationAsync("Student", id.ToString(), studentName, EntityOperation.DELETE);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting student: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete the student. Try again, and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }
        }
    }
}