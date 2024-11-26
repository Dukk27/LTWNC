using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class ProjectTask
    {
        public ProjectTask()
        {
            Documents = new HashSet<Document>();
        }

        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Priority { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreateBy { get; set; }

        public virtual Project? Project { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<ProgressReport> ProgressReports { get; set; }

    }
}
