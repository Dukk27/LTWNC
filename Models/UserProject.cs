using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class UserProject
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public bool IsPm { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
