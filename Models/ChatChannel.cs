using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class ChatChannel
    {
        public ChatChannel()
        {
            Messages = new HashSet<Message>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string? ChannelName { get; set; }
        public int ChannelType { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
