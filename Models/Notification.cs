using System;

namespace WNC_G13.Models
{
    public class Notification
    {
        public int Id { get; set; } // Khóa chính

        public int UserId { get; set; } // Mã người dùng nhận thông báo
        public int? ProjectId { get; set; } // Mã dự án (có thể null nếu thông báo không liên quan đến dự án)

        public string Message { get; set; } = null!; // Nội dung thông báo
        public bool IsRead { get; set; } = false; // Trạng thái đã đọc chưa, mặc định là false
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo thông báo

        // Mối quan hệ với bảng Users (Người dùng nhận thông báo)
        public virtual User User { get; set; } = null!;

        // Mối quan hệ với bảng Projects (Dự án liên quan)
        public virtual Project? Project { get; set; }
    }
}
