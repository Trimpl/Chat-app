using Chat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Avatar> Avatar { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
