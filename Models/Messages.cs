using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Models
{
    public class Messages
    {
        public string Id { get; set; }
        public string User1Id { get; set; }
        public string User2Id { get; set; }
        public string Contect { get; set; }
        public DateTime DateCreate { get; set; }
    }
}
