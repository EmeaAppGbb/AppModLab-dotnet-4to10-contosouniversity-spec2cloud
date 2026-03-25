using System;

namespace ContosoUniversity.Models
{
    public interface IAuditable
    {
        DateTime? CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
    }
}
