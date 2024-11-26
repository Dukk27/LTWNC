using System.Collections.Generic;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IChannelRepository
    {
        List<ChatChannel> GetChannels(); // Phương thức đồng bộ
        void CreateChannel(ChatChannel channel);
        void DeleteChannel(int channelId);
    }
}