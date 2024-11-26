using System.Collections.Generic;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IMessageRepository
    {
        List<Message> GetMessages(int channelId); // Phương thức đồng bộ
        void AddMessage(Message message); // Phương thức đồng bộ
        Message GetMessageById(int messageId);
        void DeleteMessage(int messageId);
        IEnumerable<Message> GetAllMessages();
    }
}
