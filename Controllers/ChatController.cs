using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;
using WNC_G13.Models;
using WNC_G13.Repositories;
using System.Security.Claims;

namespace WNC_G13.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IMessageRepository _chatRepository;
        private readonly IChannelRepository _channelRepository;

        public ChatController(IMessageRepository chatRepository, IChannelRepository channelRepository)
        {
            _chatRepository = chatRepository;
            _channelRepository = channelRepository;
        }

        [HttpGet]
        public IActionResult GetChannels()
        {
            var channels = _channelRepository.GetChannels();
            return Json(channels); // Trả về danh sách kênh dưới dạng JSON
        }

        public IActionResult Index(int channelId)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ChannelId = channelId;
            ViewBag.UserName = userName; // Lấy tên người dùng từ claims
            ViewBag.UserId = userId;
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var channels = _channelRepository.GetChannels();
            return View(channels); // Truyền danh sách kênh vào view
        }

        [HttpGet]
        public JsonResult GetMessages(int channelId)
        {
            var messages = _chatRepository.GetMessages(channelId)
                .Select(m => new
                {
                    m.Id,
                    m.ChannelId,
                    m.MessageContent,
                    m.CreatedAt,
                    UserId = m.UserId,
                    UserName = m.User.FullName // Lấy tên người dùng từ quan hệ
                }).ToList();

            return Json(messages);
        }

        [HttpPost]
        public JsonResult SendMessage(int channelId, string messageContent)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Người dùng không xác định." });
            }

            var message = new Message
            {
                ChannelId = channelId,
                UserId = int.Parse(userId), // Lấy UserId từ claims
                MessageContent = messageContent,
                CreatedAt = DateTime.Now
            };

            _chatRepository.AddMessage(message);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CreateChannel(ChatChannel channel)
        {
            channel.CreatedAt = DateTime.Now;
            _channelRepository.CreateChannel(channel);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteChannel(int channelId)
        {
            _channelRepository.DeleteChannel(channelId);
            return Json(new { success = true });
        }

        // Hiển thị trang quản lý tin nhắn cho admin
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult MessageManagement()
        {
            var messages = _chatRepository.GetAllMessages();
            return View(messages); // Trả về view quản lý tin nhắn
        }

        // Xóa tin nhắn
        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteMessage(int messageId)
        {
            var message = _chatRepository.GetMessageById(messageId);
            if (message == null)
            {
                return Json(new { success = false, message = "Tin nhắn không tồn tại." });
            }

            _chatRepository.DeleteMessage(messageId);

            // Trả về phản hồi JSON thành công
            return Json(new { success = true, message = "Tin nhắn đã được xóa thành công!" });
        }
    }
}
