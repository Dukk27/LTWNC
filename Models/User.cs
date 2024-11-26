using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WNC_G13.Models
{
    public partial class User
    {
        public User()
        {
            Documents = new HashSet<Document>();
            Messages = new HashSet<Message>();
            Organizations = new HashSet<Organization>();
            Projects = new HashSet<Project>();
            UserProjects = new HashSet<UserProject>();
            Channels = new HashSet<ChatChannel>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và Tên không được để trống.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string? Password { get; set; }
        public DateTime? CreatedAt { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public int Role { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }

        public virtual ICollection<ChatChannel> Channels { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
        public virtual ICollection<ProgressReport> ProgressReports { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } 

    }
}
