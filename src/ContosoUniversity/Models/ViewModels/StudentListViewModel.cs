namespace ContosoUniversity.Models.ViewModels
{
    public class StudentListViewModel
    {
        public PaginatedList<Student> Students { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public string NameSortParm { get; set; }
        public string DateSortParm { get; set; }
    }
}
