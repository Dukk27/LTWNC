using System;
using System.ComponentModel.DataAnnotations;

namespace WNC_G13.Models
{
    public class UserOrganization
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Organization Organization { get; set; }= null!;
    }
}
