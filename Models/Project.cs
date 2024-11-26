using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class Project
    {
        public Project()
        {
            Documents = new HashSet<Document>();
            ProjectTasks = new HashSet<ProjectTask>();
            UserProjects = new HashSet<UserProject>();
        }

        public int Id { get; set; }
        public int? OrganizationId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Status { get; set; }
        

        public virtual User? CreatedByNavigation { get; set; }
        public virtual Organization? Organization { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual ICollection<ProgressReport> ProgressReports { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } 

    }
}
