using System;

namespace WNC_G13.Models
{
    public class ProgressReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        public virtual User User { get; set; }
        public virtual ProjectTask? Task { get; set; }
        public virtual Project Project { get; set; }
        public virtual User? Approver { get; set; } 
    }
}
