using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class Organization
    {
        public Organization()
        {
            Projects = new HashSet<Project>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? SubscriptionPlan { get; set; }
        public long? StorageLimit { get; set; }
        public long? UsedStorage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public virtual User? CreatedByNavigation { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
         public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
