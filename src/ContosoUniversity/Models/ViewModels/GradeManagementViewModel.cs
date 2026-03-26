using System;
using System.Collections.Generic;

namespace ContosoUniversity.Models.ViewModels
{
    public class GradeManagementViewModel
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public List<EnrollmentGradeItem> Enrollments { get; set; } = new();
    }

    public class EnrollmentGradeItem
    {
        public int EnrollmentID { get; set; }
        public string StudentName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Grade? Grade { get; set; }
    }
}
