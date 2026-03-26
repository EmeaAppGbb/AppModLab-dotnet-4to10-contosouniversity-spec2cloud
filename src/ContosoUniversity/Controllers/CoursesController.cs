using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.ViewModels;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(SchoolContext context, INotificationService notificationService, IWebHostEnvironment env, ILogger<CoursesController> logger)
            : base(context, notificationService, logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var courses = db.Courses.Include(c => c.Department);
            return View(await courses.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = await db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        public IActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(new Course());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        var uploadsPath = Path.Combine(_env.WebRootPath, "Uploads", "TeachingMaterials");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await teachingMaterialImage.CopyToAsync(stream);
                        }
                        course.TeachingMaterialImagePath = $"/Uploads/TeachingMaterials/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Courses.Add(course);
                await db.SaveChangesAsync();

                await SendEntityNotificationAsync("Course", course.CourseID.ToString(), course.Title, EntityOperation.CREATE);

                return RedirectToAction("Index");
            }

            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        var uploadsPath = Path.Combine(_env.WebRootPath, "Uploads", "TeachingMaterials");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                        {
                            var oldFilePath = Path.Combine(_env.WebRootPath, course.TeachingMaterialImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await teachingMaterialImage.CopyToAsync(stream);
                        }
                        course.TeachingMaterialImagePath = $"/Uploads/TeachingMaterials/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Entry(course).State = EntityState.Modified;
                await db.SaveChangesAsync();

                await SendEntityNotificationAsync("Course", course.CourseID.ToString(), course.Title, EntityOperation.UPDATE);

                return RedirectToAction("Index");
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = await db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Course course = await db.Courses.FindAsync(id);
            var courseTitle = course.Title;

            if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, course.TeachingMaterialImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error deleting file: {ex.Message}");
                    }
                }
            }

            db.Courses.Remove(course);
            await db.SaveChangesAsync();

            await SendEntityNotificationAsync("Course", id.ToString(), courseTitle, EntityOperation.DELETE);

            return RedirectToAction("Index");
        }

        // GET: Courses/Grades/5
        public async Task<IActionResult> Grades(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var enrollments = await db.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseID == id)
                .OrderBy(e => e.Student.LastName)
                .ToListAsync();

            var viewModel = new GradeManagementViewModel
            {
                CourseID = course.CourseID,
                CourseTitle = course.Title,
                Enrollments = enrollments.Select(e => new EnrollmentGradeItem
                {
                    EnrollmentID = e.EnrollmentID,
                    StudentName = e.Student.FullName,
                    EnrollmentDate = e.Student.EnrollmentDate,
                    Grade = e.Grade
                }).ToList()
            };

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(viewModel);
        }

        // POST: Courses/SaveGrades/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(int id, GradeManagementViewModel model)
        {
            var course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            try
            {
                int updatedCount = 0;

                foreach (var item in model.Enrollments)
                {
                    var enrollment = await db.Enrollments
                        .Include(e => e.Student)
                        .Include(e => e.Course)
                        .Where(e => e.EnrollmentID == item.EnrollmentID)
                        .SingleOrDefaultAsync();

                    if (enrollment != null && enrollment.Grade != item.Grade)
                    {
                        enrollment.Grade = item.Grade;
                        updatedCount++;

                        var studentName = enrollment.Student?.FullName ?? "Unknown";
                        await SendEntityNotificationAsync("Enrollment",
                            enrollment.EnrollmentID.ToString(),
                            $"{studentName} in {course.Title}",
                            EntityOperation.UPDATE);
                    }
                }

                if (updatedCount > 0)
                {
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{updatedCount} grade(s) updated successfully.";
                }
                else
                {
                    TempData["SuccessMessage"] = "No grade changes detected.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving grades for course {CourseId}", id);
                TempData["ErrorMessage"] = "Unable to save grades. Please try again.";
            }

            return RedirectToAction("Grades", new { id });
        }
    }
}