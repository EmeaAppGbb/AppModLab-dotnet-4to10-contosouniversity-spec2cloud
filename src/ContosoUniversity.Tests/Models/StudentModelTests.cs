using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ContosoUniversity.Models;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class StudentModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void LastName_Required_FailsWhenEmpty()
        {
            var student = new Student
            {
                LastName = "",
                FirstMidName = "Test",
                EnrollmentDate = new DateTime(2024, 1, 1)
            };

            var results = ValidateModel(student);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "LastName"));
        }

        [Fact]
        public void LastName_MaxLength50_FailsWhenTooLong()
        {
            var student = new Student
            {
                LastName = new string('A', 51),
                FirstMidName = "Test",
                EnrollmentDate = new DateTime(2024, 1, 1)
            };

            var results = ValidateModel(student);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "LastName"));
        }

        [Fact]
        public void FirstMidName_Required_FailsWhenEmpty()
        {
            var student = new Student
            {
                LastName = "Test",
                FirstMidName = "",
                EnrollmentDate = new DateTime(2024, 1, 1)
            };

            var results = ValidateModel(student);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "FirstMidName"));
        }

        [Fact]
        public void EnrollmentDate_ValidDateRange_AcceptsBoundary()
        {
            var student = new Student
            {
                LastName = "Test",
                FirstMidName = "Student",
                EnrollmentDate = new DateTime(1753, 1, 1)
            };

            var results = ValidateModel(student);

            // Should not have an EnrollmentDate error for the boundary date
            Assert.DoesNotContain(results, r => r.MemberNames.Any(m => m == "EnrollmentDate"));
        }
    }
}
