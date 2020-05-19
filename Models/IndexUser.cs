using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Models
{
    public class IndexUser
    {
        public IdentityUser IdentityUser { get; set; }
        public Avatar Avatar { get; set; }
    }
}
