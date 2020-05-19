using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Chat.Hubs;
using Chat.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using Chat.ViewModels;

namespace Chat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> hubContext;

        public HomeController(
            ApplicationContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, 
            IHubContext<ChatHub> hubContext, 
            ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            this.hubContext = hubContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<IndexUser> Users = new List<IndexUser>();
            List<IdentityUser> UsersL = _userManager.Users.ToList();
            foreach (var user in UsersL) 
            {
                IndexUser User = new IndexUser();
                Avatar avatar = _context.Avatar.Find(user.Email);
                User.Avatar = avatar;
                User.IdentityUser = user;
                Users.Add(User);
            };
            IndexViewModel model = new IndexViewModel
            {
                Users = Users
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ShowChat(string toUser)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Index");
            }
            ChatViewModel model = new ChatViewModel 
            {
                Messages = _context.Messages
                .Where(x =>
                (x.User1Id == User.Identity.Name && 
                x.User2Id == toUser) ||
                (x.User2Id == User.Identity.Name &&
                x.User1Id == toUser))
                .OrderBy(x => x.DateCreate)
                .ToList(),
                toUser = toUser,
                avatarUser1 = _context.Avatar.Find(User.Identity.Name),
                avatarUser2 = _context.Avatar.Find(toUser)
            };
            return View("ShowChat", model);
        }
    }
}
