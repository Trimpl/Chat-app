using Chat.Data;
using Chat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Chat.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationContext _messages;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatHub(ApplicationContext messages,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _messages = messages;
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public async Task SendMessage(string user, string message, string to)
        {
            Messages model = new Messages
            {
                Contect = message,
                Id = Guid.NewGuid().ToString(),
                User1Id = user,
                User2Id = to,
                DateCreate = DateTime.Now
            };
            _messages.Messages.Add(model);
            await _messages.SaveChangesAsync();
            await Clients.Caller.SendAsync("ReceiveMessage", JsonConvert.SerializeObject(model));
            await Clients.User(to).SendAsync("Others", JsonConvert.SerializeObject(model));
            await Clients.User(to).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(model));
            await Clients.User(to).SendAsync("Xamarin", JsonConvert.SerializeObject(model));
        }
        public async Task Xamarin(string message, string to, string jwt)
        {
            ClaimsPrincipal claims = await ValidateAndDecode(jwt);
            Messages model = new Messages
            {
                Contect = message,
                Id = Guid.NewGuid().ToString(),
                User1Id = claims.Identity.Name,
                User2Id = to,
                DateCreate = DateTime.Now
            };
            _messages.Messages.Add(model);
            await _messages.SaveChangesAsync();
            if (claims.Identity.Name != to)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", JsonConvert.SerializeObject(model));
            };
            await Clients.User(to).SendAsync("Others", JsonConvert.SerializeObject(model));
            await Clients.User(to).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(model));
            await Clients.User(to).SendAsync("Xamarin", JsonConvert.SerializeObject(model));
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        private async Task<ClaimsPrincipal> ValidateAndDecode(string jwt)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                RequireExpirationTime = true,
            };

            try
            {
                Dictionary<string, string> tokenJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(jwt);
                string token = tokenJson["access_token"];
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out var rawValidatedToken);
                var user = await _userManager.FindByNameAsync(tokenJson["username"]);
                var result = _signInManager.SignInAsync(user, true);
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
    }
}
