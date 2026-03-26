using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ContosoUniversity.Models;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class DepartmentModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void Name_Required_FailsWhenEmpty()
        {
            var dept = new Department
            {
                Name = "",
                Budget = 100000,
                StartDate = new DateTime(2024, 1, 1)
            };

            var results = ValidateModel(dept);

            // StringLength with MinLength will fail for empty string
            Assert.Contains(results, r => r.MemberNames.Any(m => m == "Name"));
        }

        [Fact]
        public void Name_MinLength3_FailsWhenTooShort()
        {
            var dept = new Department
            {
                Name = "AB",
                Budget = 100000,
                StartDate = new DateTime(2024, 1, 1)
            };

            var results = ValidateModel(dept);

            Assert.Contains(results, r => r.MemberNames.Any(m => m == "Name"));
        }
    }
}
