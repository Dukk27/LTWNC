using System.Collections.Generic;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        // Giả sử bạn có một DbContext để truy cập dữ liệu
        private readonly WNCG13Context _context;

        public ChannelRepository(WNCG13Context context)
        {
            _context = context;
        }

        public List<ChatChannel> GetChannels()
        {
            return _context.ChatChannels.ToList(); // Lấy danh sách kênh từ database
        }

        public void CreateChannel(ChatChannel channel)
        {
            _context.ChatChannels.Add(channel);
            _context.SaveChanges();
        }

        public void DeleteChannel(int channelId)
        {
            var channel = _context.ChatChannels.Find(channelId);
            if (channel != null)
            {
                _context.ChatChannels.Remove(channel);
                _context.SaveChanges();
            }
        }
    }
}
