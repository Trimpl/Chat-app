using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chat.Data;
using Chat.Hubs;
using Chat.Models;
using Chat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Chat.Controllers
{
    public class MobileController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> hubContext;

        public MobileController(
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

        [AllowAnonymous]
        public IActionResult Token(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity.Result == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Result.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(9999)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Result.Name
            };
            return Ok(response);
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, username)
                    };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            return null;
        }

        public List<IndexUser> GetListOfUsers(string jwt)
        {
            var claimsIdentity = ValidateAndDecode(jwt);
            if (claimsIdentity != null)
            {
                List<IndexUser> listOfUsersWithAvatars = new List<IndexUser>();
                List<IdentityUser> listOfUsers = _userManager.Users.ToList();
                foreach (var user in listOfUsers)
                {
                    IndexUser User = new IndexUser();
                    Avatar avatar = _context.Avatar.Find(user.Email);
                    User.Avatar = avatar;
                    User.IdentityUser = user;
                    listOfUsersWithAvatars.Add(User);
                };
                return listOfUsersWithAvatars;
            };
            return null;
        }

        private ClaimsPrincipal ValidateAndDecode(string jwt)
        {
            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ValidateIssuer = true,
                // строка, представляющая издателя
                ValidIssuer = AuthOptions.ISSUER,

                // будет ли валидироваться потребитель токена
                ValidateAudience = true,
                // установка потребителя токена
                ValidAudience = AuthOptions.AUDIENCE,
                // будет ли валидироваться время существования
                ValidateLifetime = true,
                // установка ключа безопасности
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);
                return claimsPrincipal;
            }
            catch (SecurityTokenValidationException SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException ArgumentException)
            {
                return null;
            }
        }

        public async Task<IActionResult> GetIdentityToken(string jwt)
        {
            ClaimsPrincipal claimsIdentity = ValidateAndDecode(jwt);
            if (claimsIdentity != null)
            {
                IdentityUser user = await _userManager.FindByNameAsync(claimsIdentity.Identity.Name);
                await _signInManager.SignInAsync(user, true);
                return Ok();
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetListOfMessagges(string jwt, string toUser)
        {
            ClaimsPrincipal claims = ValidateAndDecode(jwt);
            if (claims != null)
            {
                ChatViewModel model = new ChatViewModel
                {
                    Messages = _context.Messages
                    .Where(x =>
                    (x.User1Id == claims.Identity.Name &&
                    x.User2Id == toUser) ||
                    (x.User2Id == claims.Identity.Name &&
                    x.User1Id == toUser))
                    .OrderBy(x => x.DateCreate)
                    .ToList(),
                    toUser = toUser,
                    avatarUser1 = await _context.Avatar.FindAsync(claims.Identity.Name),
                    avatarUser2 = await _context.Avatar.FindAsync(toUser)
                };
                return Ok(model);
            };
            return BadRequest();
        }
    }
}
