using System;
using System.ComponentModel.DataAnnotations;
using ContosoUniversity.Models.Validation;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class ValidDateRangeAttributeTests
    {
        private readonly ValidDateRangeAttribute _attribute = new();

        [Fact]
        public void ValidDate_ReturnsSuccess()
        {
            var result = _attribute.GetValidationResult(new DateTime(2024, 1, 1), new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void MinValueDate_ReturnsError()
        {
            var result = _attribute.GetValidationResult(DateTime.MinValue, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
        }

        [Fact]
        public void DateBefore1753_ReturnsError()
        {
            var result = _attribute.GetValidationResult(new DateTime(1752, 12, 31), new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
        }

        [Fact]
        public void BoundaryDate1753_ReturnsSuccess()
        {
            var result = _attribute.GetValidationResult(new DateTime(1753, 1, 1), new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }
    }
}
