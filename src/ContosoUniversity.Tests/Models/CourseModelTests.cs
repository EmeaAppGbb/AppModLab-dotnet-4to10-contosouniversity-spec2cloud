using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ContosoUniversity.Models;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class CourseModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void Title_MinLength3_FailsWhenTooShort()
        {
            var course = new Course
            {
                CourseID = 1,
                Title = "AB",
                Credits = 3,
                DepartmentID = 1
            };

            var results = ValidateModel(course);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "Title"));
        }

        [Fact]
        public void Title_MaxLength50_FailsWhenTooLong()
        {
            var course = new Course
            {
                CourseID = 1,
                Title = new string('A', 51),
                Credits = 3,
                DepartmentID = 1
            };

            var results = ValidateModel(course);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "Title"));
        }

        [Fact]
        public void Credits_Range0To5_FailsWhenOutOfRange()
        {
            var course = new Course
            {
                CourseID = 1,
                Title = "Valid Title",
                Credits = 6,
                DepartmentID = 1
            };

            var results = ValidateModel(course);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "Credits"));
        }

        [Fact]
        public void Credits_Range0To5_AcceptsValidValue()
        {
            var course = new Course
            {
                CourseID = 1,
                Title = "Valid Title",
                Credits = 3,
                DepartmentID = 1
            };

            var results = ValidateModel(course);

            Assert.DoesNotContain(results, r => r.MemberNames.Any(m => m == "Credits"));
        }
    }
}
