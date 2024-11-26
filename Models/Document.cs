using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class Document
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? TaskId { get; set; }
        public string? DocumentName { get; set; }
        public string? DocumentUrl { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string? Description { get; set; }

        public virtual Project? Project { get; set; }
        public virtual ProjectTask? Task { get; set; }
        public virtual User? UploadedByNavigation { get; set; }
    }
}
