using Microsoft.AspNetCore.Mvc;
using WNC_G13.Models;
using WNC_G13.Repositories;
using System;

namespace WNC_G13.Controllers
{
    public class ChannelController : Controller
    {
        private readonly IChannelRepository _channelRepository;

        public ChannelController(IChannelRepository channelRepository)
        {
            _channelRepository = channelRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ChatChannel model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _channelRepository.CreateChannel(model);
                return RedirectToAction("Index", "Chat");
            }
            return View(model);
        }

        public IActionResult Index()
        {
            var channels = _channelRepository.GetChannels();
            return View(channels);
        }
    }
}
