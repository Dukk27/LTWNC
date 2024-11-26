using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class Message
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int UserId { get; set; }
        public string MessageContent { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual ChatChannel Channel { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
