using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : BaseController
    {
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(SchoolContext context, INotificationService notificationService, ILogger<DepartmentsController> logger)
            : base(context, notificationService, logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var departments = db.Departments.Include(d => d.Administrator);
            return View(await departments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        public IActionResult Create()
        {
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Budget,StartDate,InstructorID")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();

                await SendEntityNotificationAsync("Department", department.DepartmentID.ToString(), department.Name, EntityOperation.CREATE);

                return RedirectToAction("Index");
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("DepartmentID,Name,Budget,StartDate,InstructorID,RowVersion")] Department department)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(department).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    await SendEntityNotificationAsync("Department", department.DepartmentID.ToString(), department.Name, EntityOperation.UPDATE);

                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Department)entry.Entity;
                var databaseEntry = await entry.GetDatabaseValuesAsync();

                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to save changes. The department was deleted by another user.");
                }
                else
                {
                    var databaseValues = (Department)databaseEntry.ToObject();

                    if (databaseValues.Name != clientValues.Name)
                        ModelState.AddModelError("Name", $"Current value: {databaseValues.Name}");
                    if (databaseValues.Budget != clientValues.Budget)
                        ModelState.AddModelError("Budget", $"Current value: {databaseValues.Budget:c}");
                    if (databaseValues.StartDate != clientValues.StartDate)
                        ModelState.AddModelError("StartDate", $"Current value: {databaseValues.StartDate:d}");
                    if (databaseValues.InstructorID != clientValues.InstructorID)
                    {
                        var instructor = await db.Instructors.FindAsync(databaseValues.InstructorID);
                        ModelState.AddModelError("InstructorID", $"Current value: {instructor?.FullName}");
                    }

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. The "
                        + "edit operation was canceled and the current values in the database "
                        + "have been displayed. If you still want to edit this record, click "
                        + "the Save button again. Otherwise click the Back to List hyperlink.");

                    department.RowVersion = databaseValues.RowVersion;
                }
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Department department = await db.Departments.FindAsync(id);
            var departmentName = department.Name;
            db.Departments.Remove(department);
            await db.SaveChangesAsync();

            await SendEntityNotificationAsync("Department", id.ToString(), departmentName, EntityOperation.DELETE);

            return RedirectToAction("Index");
        }
    }
}