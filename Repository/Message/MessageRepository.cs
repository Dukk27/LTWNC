using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly WNCG13Context _context;

        public MessageRepository(WNCG13Context context)
        {
            _context = context;
        }

        public List<Message> GetMessages(int channelId)
        {
           return _context.Messages
                .Include(m => m.User) // Bao gồm thông tin người dùng
                .Where(m => m.ChannelId == channelId)
                .ToList();
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
            _context.SaveChanges(); // Lưu thay đổi đồng bộ
        }

        public Message GetMessageById(int messageId)
        {
            return _context.Messages
                .FirstOrDefault(m => m.Id == messageId);
        }

        public void DeleteMessage(int messageId)
        {
            var message = GetMessageById(messageId);
            if (message != null)
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Message> GetAllMessages()
    {
        return _context.Messages
                       .Include(m => m.Channel)
                       .Include(m => m.User)
                       .ToList();
    }
    }
}
